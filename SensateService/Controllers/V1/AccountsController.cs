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
using Newtonsoft.Json;

using Swashbuckle.AspNetCore.SwaggerGen;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Services;
using SensateService.Models.Json.In;
using SensateService.Attributes;
using SensateService.Models.Json.Out;
using SensateService.Enums;

namespace SensateService.Controllers.V1
{
	[Produces("application/json")]
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class AccountsController : AbstractController
	{
		private readonly UserAccountSettings _settings;
		private readonly UserManager<SensateUser> _manager;
		private readonly IEmailSender _mailer;
		private readonly IPasswordResetTokenRepository _passwd_tokens;
		private readonly IChangeEmailTokenRepository _email_tokens;
		private readonly IHostingEnvironment _env;
		private readonly IAuditLogRepository _audit_logs;

		public AccountsController(
			IUserRepository repo,
			SignInManager<SensateUser> manager,
			UserManager<SensateUser> userManager,
			IOptions<UserAccountSettings> options,
			IEmailSender emailer,
			IPasswordResetTokenRepository tokens,
			IChangeEmailTokenRepository emailTokens,
			IAuditLogRepository auditLogs,
			IHostingEnvironment env
		) : base(repo)
		{
			this._settings = options.Value;
			this._manager = userManager;
			this._mailer = emailer;
			this._passwd_tokens = tokens;
			this._email_tokens = emailTokens;
			this._env = env;
			this._audit_logs = auditLogs;
		}

		[HttpPost("forgot-password")]
		[ValidateModel]
		[SwaggerResponse(200)]
		[SwaggerResponse(404)]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword model)
		{
			SensateUser user;
			string usertoken;
			EmailBody mail;

			user = await this._users.GetByEmailAsync(model.Email);
			await this.Log(RequestMethod.HttpPost, user);

			if(user == null || !user.EmailConfirmed)
				return NotFound();

			mail = await this.ReadMailTemplate("Confirm_Password_Reset.html", "Confirm_Password_Reset.txt");
			var token = await this._manager.GeneratePasswordResetTokenAsync(user);
			token = Base64UrlEncoder.Encode(token);
			usertoken = this._passwd_tokens.Create(token);

			if(usertoken == null)
				return this.StatusCode(500);

			mail.HtmlBody = mail.HtmlBody.Replace("%%TOKEN%%", usertoken);
			mail.TextBody = String.Format(mail.TextBody, usertoken);
			await this._mailer.SendEmailAsync(user.Email, "Reset password token", mail);
			return Ok();
		}

		[HttpPost("reset-password")]
		[SwaggerResponse(200)]
		[SwaggerResponse(404)]
		[ValidateModel]
		public async Task<IActionResult> Resetpassword([FromBody] ResetPassword model)
		{
			SensateUser user;
			PasswordResetToken token;

			user = await this._users.GetByEmailAsync(model.Email);
			token = this._passwd_tokens.GetById(model.Token);
			await this.Log(RequestMethod.HttpPost, user);

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
		[ValidateModel]
		[SwaggerResponse(200)]
		[SwaggerResponse(400)]
		public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmUpdateEmail changeEmail)
		{
			ChangeEmailToken token;

			if(changeEmail.Token == null || changeEmail.Token.Length == 0) {
				await this.Log(RequestMethod.HttpPost);
				return BadRequest();
			}

			var user = await this.GetCurrentUserAsync();
			await this.Log(RequestMethod.HttpPost, user);
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
		[ValidateModel]
		[SwaggerResponse(200)]
		[SwaggerResponse(400)]
		[NormalUser]
		public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmail changeEmailModel)
		{
			string token;
			string resetToken;
			SensateUser user;
			EmailBody mail;

			if(changeEmailModel.NewEmail == null || changeEmailModel.NewEmail.Length == 0) {
				await this.Log(RequestMethod.HttpPost);
				return BadRequest();
			}

			user = await this.GetCurrentUserAsync();
			await this.Log(RequestMethod.HttpPost, user);

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

		[HttpGet("roles")]
		[NormalUser]
		[ProducesResponseType(typeof(UserRoles), 200)]
		public async Task<IActionResult> GetRoles()
		{
			var user = await this.GetCurrentUserAsync();
			IList<string> roles;

			roles = await this._users.GetRolesAsync(user) as IList<string>;
			var reply = new UserRoles {
				Roles = roles,
				Email = user.Email
			};

			return new OkObjectResult(reply);
		}


		private async Task<EmailBody> ReadMailTemplate(string html, string text)
		{
			EmailBody body;
			string path;

			body = new EmailBody();
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
		[ValidateModel]
		[SwaggerResponse(200)]
		[SwaggerResponse(400)]
		public async Task<object> Register([FromBody] Register register)
		{
			EmailBody mail;
			var user = new SensateUser {
				UserName = register.Email,
				Email = register.Email,
				FirstName = register.FirstName,
				LastName = register.LastName,
				PhoneNumber = register.PhoneNumber
			};

			await this.Log(RequestMethod.HttpPost);
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
		[SwaggerResponse(404)]
		[ProducesResponseType(typeof(User), 200)]
		[NormalUser]
		public async Task<IActionResult> Show()
		{
			User viewuser;
			var user = await this.GetCurrentUserAsync();

			await this.Log(RequestMethod.HttpGet, user);

			viewuser = new User {
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PhoneNumber = user.PhoneNumber
			};

			return new ObjectResult(viewuser);
		}

		[HttpGet("confirm/{id}/{code}")]
		[SwaggerResponse(200)]
		[SwaggerResponse(404)]
		public async Task<IActionResult> ConfirmEmail(string id, string code)
		{
			SensateUser user;

			await this.Log(RequestMethod.HttpGet);

			if(id == null || code == null) {
				return BadRequest();
			}

			user = await this._users.GetAsync(id);
			if(user == null)
				return NotFound();

			/*
			 * For some moronic reason we need to encode and decode to
			 * Base64. The + sign gets mangled to a space if we don't.
			 */
			code = Base64UrlEncoder.Decode(code);
			var result = await this._manager.ConfirmEmailAsync(user, code);
			if(!result.Succeeded)
				return this.InvalidInputResult();

			return this.Ok();
		}

		[ValidateModel]
		[NormalUser]
		[HttpPatch("update")]
		[ProducesResponseType(typeof(Status), 400)]
		[SwaggerResponse(200)]
		public async Task<IActionResult> UpdateUser([FromBody] UpdateUser userUpdate)
		{
			var user = this.CurrentUser;

			await this.Log(RequestMethod.HttpPatch, user);

			if(userUpdate.Password != null) {
				if(userUpdate.CurrentPassword == null)
					return this.InvalidInputResult("Current password not given");

				var result = await this._manager.ChangePasswordAsync(user,
					userUpdate.CurrentPassword, userUpdate.Password);
				if(!result.Succeeded)
					return this.InvalidInputResult(result.Errors.First().Description);
			}

			this._users.StartUpdate(user);

			if(userUpdate.FirstName != null)
				user.FirstName = userUpdate.FirstName;

			if(userUpdate.LastName != null)
				user.LastName = userUpdate.LastName;

			await this._users.EndUpdateAsync();

			if(userUpdate.PhoneNumber != null)
				await this._manager.SetPhoneNumberAsync(user, userUpdate.PhoneNumber);

			return Ok();
		}

		private async Task Log(RequestMethod method, SensateUser user = null)
		{
			await this._audit_logs.CreateAsync(
				this.GetCurrentRoute(), method,
				this.GetRemoteAddress(), user
			);
		}
	}
}
