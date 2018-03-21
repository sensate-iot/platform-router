/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

using Newtonsoft.Json.Linq;
using MimeKit;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json;
using SensateService.Services;

namespace SensateService.Controllers
{
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class AccountsController : AbstractController
	{
		private readonly UserAccountSettings _settings;
		private readonly SignInManager<SensateUser> _siManager;
		private readonly UserManager<SensateUser> _manager;
		private readonly IUserRepository _users;
		private readonly IEmailSender _mailer;
		private readonly IPasswordResetTokenRepository _tokens;
		private readonly IChangeEmailTokenRepository _email_tokens;
		private readonly IHostingEnvironment _env;

		public AccountsController(
			IUserRepository repo,
			SignInManager<SensateUser> manager,
			UserManager<SensateUser> userManager,
			IOptions<UserAccountSettings> options,
			IEmailSender emailer,
			IPasswordResetTokenRepository tokens,
			IChangeEmailTokenRepository emailTokens,
			IHostingEnvironment env
		)
		{
			this._users = repo;
			this._siManager = manager;
			this._settings = options.Value;
			this._manager = userManager;
			this._mailer = emailer;
			this._tokens = tokens;
			this._email_tokens = emailTokens;
			this._env = env;
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword model)
		{
			SensateUser user;
			string usertoken;
			BodyBuilder mail;

			user = await this._users.GetByEmailAsync(model.Email);
			if(user == null || !user.EmailConfirmed)
				return NotFound();

			mail = await this.ReadMailTemplate("Confirm_Password_Reset.html", "Confirm_Password_Reset.txt");
			var token = await this._manager.GeneratePasswordResetTokenAsync(user);
			token = Base64UrlEncoder.Encode(token);
			usertoken = this._tokens.Create(token);

			if(usertoken == null)
				return this.StatusCode(500);

			mail.HtmlBody = mail.HtmlBody.Replace("%%TOKEN%%", usertoken);
			mail.TextBody = String.Format(mail.TextBody, usertoken);
			await this._mailer.SendEmailAsync(user.Email, "Reset password token", mail);
			return Ok();
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> Resetpassword([FromBody] ResetPassword model)
		{
			SensateUser user;
			PasswordResetToken token;

			if(model.Email == null || model.Password == null || model.Token == null)
				return BadRequest();

			user = await this._users.GetByEmailAsync(model.Email);
			token = this._tokens.GetById(model.Token);

			if(user == null || token == null)
				return NotFound();

			token.IdentityToken = Base64UrlEncoder.Decode(token.IdentityToken);
			var result = await this._manager.ResetPasswordAsync(user, token.IdentityToken, model.Password);

			if(result.Succeeded)
				return Ok();

			return new NotFoundObjectResult(new {Message = result.Errors});
		}

		[HttpPost("confirm-update-email")]
		[Authorize]
		public async Task<IActionResult> ConfirmChangeEmail([FromBody] UpdateEmail changeEmail)
		{
			ChangeEmailToken token;

			if(changeEmail.Email == null || changeEmail.Email.Length == 0 ||
				changeEmail.Token == null || changeEmail.Token.Length == 0) {
				return BadRequest();
			}

			var user = await this._users.GetByClaimsPrincipleAsync(User);
			token = this._email_tokens.GetById(changeEmail.Token);

			if(token == null)
				return NotFound();
	
			var result = await this._manager.ChangeEmailAsync(user, token.Email, token.IdentityToken);
			await this._manager.SetUserNameAsync(user, token.Email);

			if(!result.Succeeded) {
				return this.StatusCode(500);
			}

			return this.Ok();
		}

		[HttpPost("update-email")]
		[Authorize]
		public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmail changeEmailModel)
		{
			string token;
			string resetToken;
			BodyBuilder mail;
			SensateUser user;

			if(changeEmailModel.Email == null || changeEmailModel.NewEmail == null ||
				changeEmailModel.Email.Length == 0 || changeEmailModel.NewEmail.Length == 0) {
				return BadRequest();
			}

			user = await this._users.GetByClaimsPrincipleAsync(User);
			if(user == null)
				return BadRequest();

			resetToken = await this._manager.GenerateChangeEmailTokenAsync(user, changeEmailModel.NewEmail);
			token = this._email_tokens.Create(resetToken, changeEmailModel.NewEmail);
			mail = await this.ReadMailTemplate("Confirm_Update_Email.html", "Confirm_Update_Email.txt");

			if(mail == null)
				return this.StatusCode(500);

			mail.HtmlBody = mail.HtmlBody.Replace("%%TOKEN%%", token);
			mail.TextBody = String.Format(mail.TextBody, token);
			await this._mailer.SendEmailAsync(changeEmailModel.NewEmail, "Confirm your new mail", mail);

			return this.Ok();
		}

		[HttpPost("login")]
		public async Task<object> Login([FromBody] Login loginModel)
		{
			var result = await this._siManager.PasswordSignInAsync(
				loginModel.Email,
				loginModel.Password,
				false,
				false
			);

			if(result.Succeeded) {
				var user = await this._users.GetByEmailAsync(loginModel.Email);
				return await this.GenerateJwtToken(loginModel.Email, user);
			}

			return NotFound();
		}

		private bool ValidateUser(SensateUser user)
		{
			if(user.FirstName == null || user.FirstName.Length == 0)
				return false;

			if(user.LastName == null || user.LastName.Length == 0)
				return false;

			return true;
		}

		private async Task<BodyBuilder> ReadMailTemplate(string html, string text)
		{
			BodyBuilder body;
			string path;

			body = new BodyBuilder();
			path = this._env.GetTemplatePath(html);

			using(var reader = System.IO.File.OpenText(path)) {
				body.HtmlBody = await reader.ReadToEndAsync();
			}

			path = this._env.GetTemplatePath(text);
			using(var reader = System.IO.File.OpenText(path)) {
				body.TextBody = await reader.ReadToEndAsync();
			}

			return body;
		}

		[HttpPost("register")]
		public async Task<object> Register([FromBody] Register register)
		{
			BodyBuilder mail;
			var user = new SensateUser {
				UserName = register.Email,
				Email = register.Email,
				FirstName = register.FirstName,
				LastName = register.LastName,
				PhoneNumber = register.PhoneNumber
			};

			if(!this.ValidateUser(user))
				return BadRequest();

			var result = await this._manager.CreateAsync(user, register.Password);

			if(result.Succeeded) {
				mail = await this.ReadMailTemplate("Confirm_Account_Registration.html", "Confirm_Account_Registration.txt");
				user = await this._users.GetAsync(user.Id);
				var code = await this._manager.GenerateEmailConfirmationTokenAsync(user);
				code = Base64UrlEncoder.Encode(code);
				var url = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
				mail.HtmlBody = mail.HtmlBody.Replace("%%URL%%", url);
				mail.TextBody = String.Format(mail.TextBody, url);

				await this._manager.AddToRoleAsync(user, "Users");
				await this._mailer.SendEmailAsync(user.Email, "Sensate email confirmation", mail);
				return Ok();
			}

			return BadRequest();
		}

		[HttpGet("show")]
		[Authorize]
		public async Task<IActionResult> Show()
		{
			dynamic jobj;
			var user = await this._users.GetByClaimsPrincipleAsync(User);

			if(user == null)
				return NotFound();

			jobj = new JObject();

			jobj.FirstName = user.FirstName;
			jobj.LastName = user.LastName;
			jobj.Email = user.Email;
			jobj.PhoneNumber = user.PhoneNumber ?? "";

			return new ObjectResult(jobj);
		}

		[HttpGet("confirm/{id}/{code}")]
		public async Task<IActionResult> ConfirmEmail(string id, string code)
		{
			SensateUser user;

			if(id == null || code == null) {
				return BadRequest();
			}

			user = await this._users.GetAsync(id);
			if(user == null)
				return NotFound();

			/*
			 * For some moronic reason we need to encode and decode to
			 * Base64. The + sign gets * mangled to a ' ' if we don't.
			 */
			code = Base64UrlEncoder.Decode(code);
			var result = await this._manager.ConfirmEmailAsync(user, code);
			if(!result.Succeeded)
				return Unauthorized();

			return this.Ok();
		}

		private async Task<object> GenerateJwtToken(string email, SensateUser user)
		{
			List<Claim> claims;
			JwtSecurityToken token;
			List<string> roles;

			claims = new List<Claim> {
				new Claim(JwtRegisteredClaimNames.Sub, email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id)
			};

			roles = await this._users.GetRolesAsync(user) as List<string>;
			roles.ForEach(x => {
				claims.Add(new Claim(ClaimTypes.Role, x));
			});

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._settings.JwtKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.Now.AddMinutes(this._settings.JwtExpireMinutes);
			token = new JwtSecurityToken(
				issuer: this._settings.JwtIssuer,
				audience: this._settings.JwtIssuer,
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
