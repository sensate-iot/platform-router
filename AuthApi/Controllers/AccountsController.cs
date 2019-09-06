/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.AuthApi.Helpers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Generic;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.AuthApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class AccountsController : AbstractController
	{
		private readonly UserManager<SensateUser> _manager;
		private readonly UserAccountSettings _settings;
		private readonly IEmailSender _mailer;
		private readonly IPasswordResetTokenRepository _passwd_tokens;
		private readonly IChangeEmailTokenRepository _email_tokens;
		private readonly IHostingEnvironment _env;
		private readonly IUserTokenRepository _tokens;
		private readonly ITextSendService _text;
		private readonly IChangePhoneNumberTokenRepository _phonetokens;
		private readonly TextServiceSettings _text_settings;
		private readonly ILogger<AccountsController> _logger;

		public AccountsController(
			IUserRepository repo,
			SignInManager<SensateUser> manager,
			UserManager<SensateUser> userManager,
			IEmailSender emailer,
			IOptions<UserAccountSettings> options,
			IPasswordResetTokenRepository tokens,
			IChangeEmailTokenRepository emailTokens,
			IChangePhoneNumberTokenRepository phoneTokens,
			IUserTokenRepository tokenRepository,
			ITextSendService text,
			IOptions<TextServiceSettings> text_opts,
			IHostingEnvironment env,
			IHttpContextAccessor ctx,
			ILogger<AccountsController> logger
		) : base(repo, ctx)
		{
			this._logger = logger;
			this._manager = userManager;
			this._mailer = emailer;
			this._passwd_tokens = tokens;
			this._email_tokens = emailTokens;
			this._env = env;
			this._tokens = tokenRepository;
			this._phonetokens = phoneTokens;
			this._settings = options.Value;
			this._text = text;
			this._text_settings = text_opts.Value;
		}

		[HttpPost("forgot-password")]
		[ValidateModel]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword model)
		{
			SensateUser user;
			string usertoken, token;
			EmailBody mail;

			user = await this._users.GetByEmailAsync(model.Email);

			if(user == null || !user.EmailConfirmed) {
				return NotFound();
			}

			var mailTask = this.ReadMailTemplate("Confirm_Password_Reset.html", "Confirm_Password_Reset.txt");
			token = await this._manager.GeneratePasswordResetTokenAsync(user);
			token = Base64UrlEncoder.Encode(token);
			usertoken = this._passwd_tokens.Create(token);

			if(usertoken == null)
				return this.StatusCode(500);

			mail = await mailTask.AwaitBackground();
			mail.HtmlBody = mail.HtmlBody.Replace("%%TOKEN%%", usertoken);
			mail.TextBody = string.Format(mail.TextBody, usertoken);
			await this._mailer.SendEmailAsync(user.Email, "Reset password token", mail);
			return Ok();
		}

		[HttpPost("reset-password")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		[ValidateModel]
		public async Task<IActionResult> Resetpassword([FromBody] ResetPassword model)
		{
			SensateUser user;
			PasswordResetToken token;

			user = await this._users.GetByEmailAsync(model.Email).AwaitBackground();
			token = this._passwd_tokens.GetById(model.Token);

			if(user == null)
				return this.NotFound();

			if(token == null)
				return this.InvalidInputResult("Security token invalid!");

			token.IdentityToken = Base64UrlEncoder.Decode(token.IdentityToken);
			var result = await this._manager.ResetPasswordAsync(user, token.IdentityToken, model.Password).AwaitBackground();

			if(result.Succeeded)
				return Ok();

			var error = result.Errors.First();
			return error != null ? this.InvalidInputResult(error.Description) :
				new NotFoundObjectResult(new {Message = result.Errors});
		}

		[HttpGet("phone-confirmed")]
		[NormalUser]
		[ProducesResponseType(typeof(Status), 200)]
		public async Task<IActionResult> PhoneNumberConfirmed()
		{
			SensateUser user;
			Status status;
			bool unconfirmed;

			user = await this.GetCurrentUserAsync().AwaitBackground();

			if(user == null) {
				return this.Forbid();
			}

			unconfirmed = ! await this._manager.IsPhoneNumberConfirmedAsync(user).AwaitBackground();
			unconfirmed = unconfirmed || user.UnconfirmedPhoneNumber?.Length > 0;

			status = new Status();

			if(!unconfirmed) {
				status.ErrorCode = ReplyCode.Ok;
				status.Message = "true";
			} else {
				status.ErrorCode = ReplyCode.Ok;
				status.Message = "false";
			}

			return new OkObjectResult(status);
		}

		[HttpPost("confirm-update-email")]
		[ValidateModel]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmUpdateEmail changeEmail)
		{
			ChangeEmailToken token;
			IEnumerable<UserToken> tokens;

			if(String.IsNullOrEmpty(changeEmail.Token)) {
				return BadRequest();
			}

			var user = await this.GetCurrentUserAsync();

			if(user == null)
				return this.NotFound();

			token = this._email_tokens.GetById(changeEmail.Token);
			tokens = this._tokens.GetByUser(user);

			if(token == null) {
				return this.InvalidInputResult("Token not found!");
			}

			var result = await this._manager.ChangeEmailAsync(user, token.Email, token.IdentityToken);
			await this._manager.SetUserNameAsync(user, token.Email).AwaitBackground();

			if(!result.Succeeded) {
				return this.StatusCode(500);
			}

			if(tokens != null)
				await this._tokens.InvalidateManyAsync(tokens).AwaitBackground();

			return this.Ok();
		}

		[HttpPost("update-email")]
		[ValidateModel]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[NormalUser]
		public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmail changeEmailModel)
		{
			string token;
			string resetToken;
			SensateUser user;
			EmailBody mail;

			if(string.IsNullOrEmpty(changeEmailModel.NewEmail)) {
				return BadRequest();
			}

			user = await this.GetCurrentUserAsync().AwaitBackground();

			resetToken = await this._manager.GenerateChangeEmailTokenAsync(user, changeEmailModel.NewEmail).AwaitBackground();
			token = this._email_tokens.Create(resetToken, changeEmailModel.NewEmail);
			mail = await this.ReadMailTemplate("Confirm_Update_Email.html", "Confirm_Update_Email.txt").AwaitBackground();

			if(mail == null)
				return this.StatusCode(500);

			mail.HtmlBody = mail.HtmlBody.Replace("%%TOKEN%%", token);
			mail.TextBody = String.Format(mail.TextBody, token);
			await this._mailer.SendEmailAsync(changeEmailModel.NewEmail, "Confirm your new mail", mail).AwaitBackground();

			return this.Ok();
		}

		[HttpGet("roles")]
		[NormalUser]
		[ProducesResponseType(typeof(UserRoles), 200)]
		public async Task<IActionResult> GetRoles()
		{
			var user = await this.GetCurrentUserAsync();
			IList<string> roles;

			roles = await this._users.GetRolesAsync(user).AwaitBackground() as IList<string>;
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
				body.HtmlBody = await reader.ReadToEndAsync().AwaitBackground();
			}

			path = this._env.GetTemplatePath(text);
			using(var reader = System.IO.File.OpenText(path)) {
				body.TextBody = await reader.ReadToEndAsync().AwaitBackground();
			}

			return body;
		}

		private async Task<string> ReadTextTemplate(string template, string token)
		{
			string body;
			string path;

			path = this._env.GetTemplatePath(template);
			using(var reader = System.IO.File.OpenText(path)) {
				body = await reader.ReadLineAsync().AwaitBackground();
			}

			return body == null ? null : string.Format(body, token);
		}

		private static Status StringifyIdentityResult(IdentityResult results)
		{
			Status status;

			status = new Status {
				ErrorCode = ReplyCode.BadInput,
				Message = results.Errors.ElementAt(0).Description
			};

			return status;
		}

		[HttpPost("register")]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(200)]
		public async Task<object> Register([FromBody] Register register)
		{
			EmailBody mail;
			string phonetoken;
			string usertoken;

			var user = new SensateUser {
				UserName = register.Email,
				Email = register.Email,
				FirstName = register.FirstName,
				LastName = register.LastName,
				RegisteredAt = DateTime.Now,
				UnconfirmedPhoneNumber = register.PhoneNumber
			};

			if(!this.IsValidUri(register.ForwardTo))
				return this.InvalidInputResult("Invalid forward URL!");

			var valid = await this._text.IsValidNumber(register.PhoneNumber);
			if(!valid)
				return this.InvalidInputResult("Invalid phone number!");

			var result = await this._manager.CreateAsync(user, register.Password).AwaitBackground();

			if(!result.Succeeded) {
				var objresult = StringifyIdentityResult(result);
				return this.BadRequest(objresult);
			}

			var mailTask = this.ReadMailTemplate("Confirm_Account_Registration.html", "Confirm_Account_Registration.txt");
			user = await this._users.GetAsync(user.Id).AwaitBackground();
			var code = await this._manager.GenerateEmailConfirmationTokenAsync(user).AwaitBackground();
			code = Base64UrlEncoder.Encode(code);
			var url = this.Url.EmailConfirmationLink(user.Id, code, this._settings.Scheme, this._settings.PublicUrl, register.ForwardTo);

			mail = await mailTask.AwaitBackground();
			mail.HtmlBody = mail.HtmlBody.Replace("%%URL%%", url);
			mail.TextBody = string.Format(mail.TextBody, url);

			var updates = new[] {
				this._manager.AddToRoleAsync(user, "Users"),
				this._mailer.SendEmailAsync(user.Email, "Sensate email confirmation", mail),
			};

			await Task.WhenAll(updates);
			phonetoken = await this._manager.GenerateChangePhoneNumberTokenAsync(user, register.PhoneNumber).AwaitBackground();
			usertoken = await this._phonetokens.CreateAsync(user, phonetoken, register.PhoneNumber).AwaitBackground();
			Debug.WriteLine($"Generated tokens: [identity: ${phonetoken}] [user: {usertoken}]");

			return this.Ok();
		}

		[HttpGet("show/{uid}")]
		[ProducesResponseType(404)]
		[ProducesResponseType(typeof(User), 200)]
		[AdministratorUser]
		public async Task<IActionResult> Show(string uid)
		{
			User viewuser;
			var user = await this._users.GetAsync(uid);

			if(user == null)
				return Forbid();

			viewuser = new User {
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PhoneNumber = user.PhoneNumber,
				Id = user.Id,
				RegisteredAt = user.RegisteredAt.ToUniversalTime(),
				Roles = this._users.GetRoles(user)
			};

			return new ObjectResult(viewuser);
		}

		[HttpGet("show")]
		[ProducesResponseType(404)]
		[ProducesResponseType(typeof(User), 200)]
		[NormalUser]
		public async Task<IActionResult> Show()
		{
			User viewuser;
			var user = await this.GetCurrentUserAsync().AwaitBackground();

			viewuser = new User {
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PhoneNumber = user.PhoneNumber,
				Id = user.Id,
				RegisteredAt = user.RegisteredAt.ToUniversalTime(),
				Roles = await this._users.GetRolesAsync(user)
			};

			return new ObjectResult(viewuser);
		}

		[HttpGet("confirm/{id}/{code}")]
		[ProducesResponseType(404)]
		[ProducesResponseType(200)]
		public async Task<IActionResult> ConfirmEmail(string id, string code, [FromQuery(Name = "target")] string target)
		{
			SensateUser user;
			ChangePhoneNumberToken token;
			string url, body;

			url = target != null ? WebUtility.UrlDecode(target) : null;

			if(id == null || code == null) {
				return BadRequest();
			}

			user = await this._users.GetAsync(id);
			token = await this._phonetokens.GetLatest(user);

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

			/* Send phone number validation token */
			body = await this.ReadTextTemplate("Confirm_PhoneNumber.txt", token.UserToken);
			this._text.Send(this._text_settings.AlphaCode, token.PhoneNumber, body);

			if(url != null)
				return this.Redirect(url);

			return Ok();
		}

		[NormalUser]
		[HttpGet("confirm-phone-number/{token}")]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(200)]
		public async Task<IActionResult> ConfirmPhoneNumber(string token)
		{
			SensateUser user;
			ChangePhoneNumberToken phonetoken;

			user = this.CurrentUser;

			if(user.UnconfirmedPhoneNumber == null) {
				return this.InvalidInputResult("No confirmable phone number found!");
			}

			phonetoken = await this._phonetokens.GetLatest(user);
			if(phonetoken.UserToken != token)
				return this.InvalidInputResult("Invalid confirmation token!");

			var result = await this._manager.ChangePhoneNumberAsync(user, phonetoken.PhoneNumber, phonetoken.IdentityToken).AwaitBackground();
			if(!result.Succeeded)
				return new BadRequestObjectResult(StringifyIdentityResult(result));

			this._users.StartUpdate(user);
			user.UnconfirmedPhoneNumber = null;
			await this._users.EndUpdateAsync().AwaitBackground();

			return this.Ok();
		}

		[ValidateModel, NormalUser]
		[HttpPatch("update-phone-number")]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(typeof(Status), 200)]
		public async Task<IActionResult> UpdatePhoneNumber([FromBody] PhoneNumberUpdate update)
		{
			SensateUser user;
			Status status;
			string body, phonetoken, usertoken;

			user = this.CurrentUser;

			if(user == null)
				return this.Unauthorized();

			if(update.PhoneNumber != null && update.PhoneNumber != user.PhoneNumber) {
				var valid = await this._text.IsValidNumber(update.PhoneNumber);

				if(!valid)
					return this.InvalidInputResult("Invalid phone number");
			}

			if(update.PhoneNumber != null && update.PhoneNumber != user.PhoneNumber) {
				phonetoken = await this._manager.GenerateChangePhoneNumberTokenAsync(user, update.PhoneNumber).AwaitBackground();
				usertoken = await this._phonetokens.CreateAsync(user, phonetoken, update.PhoneNumber).AwaitBackground();
				var worker = this.ReadTextTemplate("Confirm_PhoneNumber.txt", usertoken);

				this._users.StartUpdate(user);
				user.UnconfirmedPhoneNumber = update.PhoneNumber;
				await this._users.EndUpdateAsync().AwaitBackground();

				body = await worker.AwaitBackground();
				this._text.Send(this._text_settings.AlphaCode, update.PhoneNumber, body);
			}

			status = new Status {
				ErrorCode = ReplyCode.Ok,
				Message = "Phone number updated."
			};

			return this.Ok(status);
		}

		[AdministratorUser, ValidateModel]
		[HttpPost("update-roles")]
		public async Task<IActionResult> SetRoles([FromBody] IList<SetRole> userroles)
		{
			foreach(var role in userroles) {
				var user = await this._users.GetAsync(role.UserId);
				var roles = role.Role.Split(',');

				bool status = await this._users.ClearRolesForAsync(user);

				if(!status)
					return this.StatusCode(500);

				status = await this._users.AddToRolesAsync(user, roles);

				if(!status)
					return this.StatusCode(500);

				var dbgroles = await this._users.GetRolesAsync(user);
				this._logger.LogInformation($"New roles for {role.UserId}:");
				foreach(var r in dbgroles) {
					this._logger.LogInformation("\t" + r);
				}
			}

			return this.Ok();
		}

		[ValidateModel]
		[NormalUser]
		[HttpPatch("update")]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(404)]
		[ProducesResponseType(200)]
		public async Task<IActionResult> UpdateUser([FromBody] UpdateUser userUpdate)
		{
			var user = this.CurrentUser;

			if(user == null)
				return BadRequest();

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

			await this._users.EndUpdateAsync().AwaitBackground();
			return Ok();
		}
	}
}