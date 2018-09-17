/*
 * Authentication token controller.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

using SensateService.Attributes;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Sql;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;
using SensateService.Enums;
using SensateService.Helpers;

namespace SensateService.Controllers.V1
{
	[Produces("application/json")]
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class TokensController : AbstractController
	{
		private readonly IUserTokenRepository _tokens;
		private readonly SignInManager<SensateUser> _signin_manager;
		private readonly UserAccountSettings _settings;
		private readonly IAuditLogRepository _audit_log;

		public TokensController(
			IUserTokenRepository tokens,
			IOptions<UserAccountSettings> options,
			IUserRepository users,
			SignInManager<SensateUser> signInManager,
			IAuditLogRepository auditLog
		) : base(users)
		{
			this._tokens = tokens;
			this._signin_manager = signInManager;
			this._settings = options.Value;
			this._audit_log = auditLog;
		}

		[HttpPost("request")]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(typeof(TokenRequestReply), 400)]
		public async Task<ActionResult> RequestToken([FromBody] Login login)
		{
			var user = await this._users.GetByEmailAsync(login.Email).AwaitSafely();
			bool result;
			Microsoft.AspNetCore.Identity.SignInResult signInResult;
			UserToken token;

			if(user == null)
				return NotFound();

			result = await this._signin_manager.CanSignInAsync(user);

			if(!result) {
				var status = new Status {
					ErrorCode = ReplyCode.NotAllowed,
					Message = "Not allowed to sign in!"
				};
				return new BadRequestObjectResult(status);
			}

			signInResult = await this._signin_manager.PasswordSignInAsync(user, login.Password, false, false);

			if(!signInResult.Succeeded) {
				await this._audit_log.CreateAsync(
					this.GetCurrentRoute(), RequestMethod.HttpPost,
					GetRemoteAddress()
				).AwaitSafely();

				return new UnauthorizedResult();
			}

			token = this.CreateUserTokenEntry(user);
			await this._audit_log.CreateAsync(
				this.GetCurrentRoute(), RequestMethod.HttpPost,
				GetRemoteAddress(), user
			);
			await this._tokens.CreateAsync(token);

			var roles = this._users.GetRoles(user);
			var reply = new TokenRequestReply {
				RefreshToken = token.Value,
				ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes,
				JwtToken = this._tokens.GenerateJwtToken(user, roles, _settings),
				JwtExpiresInMinutes = this._settings.JwtExpireMinutes
			};

			return new OkObjectResult(reply);
		}

		[HttpPost("refresh")]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(typeof(TokenRequestReply), 400)]
		public async Task<ActionResult> RefreshToken([FromBody] RefreshLogin login)
		{
			var user = await this._users.GetByEmailAsync(login.Email).AwaitSafely();
			TokenRequestReply reply;
			UserToken token;

			if(user == null)
				return Forbid();

			token = this._tokens.GetById(user, login.RefreshToken);
			var logTask = this._audit_log.CreateAsync(this.GetCurrentRoute(),
				RequestMethod.HttpPost, GetRemoteAddress(), user);

			if(token == null || !token.Valid) {
				await logTask.AwaitSafely();
				return Forbid();
			}

			if(token.ExpiresAt < DateTime.Now) {
				await this._tokens.InvalidateTokenAsync(token).AwaitSafely();
				await logTask.AwaitSafely();
				return Forbid();
			}

			reply = new TokenRequestReply();
			var newToken = this.CreateUserTokenEntry(user);
			var tasks = new[] {
                this._tokens.CreateAsync(newToken),
                this._tokens.InvalidateTokenAsync(token)
			};

			reply.RefreshToken = newToken.Value;
			reply.ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes;
			reply.JwtToken = this._tokens.GenerateJwtToken(
				user, this._users.GetRoles(user), this._settings
			);

			await Task.WhenAll(tasks);
            await logTask.AwaitSafely();

			return new OkObjectResult(reply);
		}

		[HttpDelete("{token}", Name = "RevokeToken")]
		[NormalUser]
		[ProducesResponseType(typeof(Status), 404)]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Revoke(string token)
		{
			UserToken authToken;
			var user = await this.GetCurrentUserAsync().AwaitSafely();

			if(user == null)
				return Forbid();

			if(String.IsNullOrEmpty(token))
				return InvalidInputResult("Token not found!");

			authToken = this._tokens.GetById(user, token);
			var tasks = new Task[2];

			tasks[0] = this._audit_log.CreateAsync(
				this.GetCurrentRoute(),
				RequestMethod.HttpDelete,
				GetRemoteAddress(),
				user
			);

			if(authToken == null)
				return this.NotFoundInputResult("Token not found!");

			if(!authToken.Valid)
				return this.InvalidInputResult("Token already invalid!");

			tasks[1] = this._tokens.InvalidateTokenAsync(authToken);
			await Task.WhenAll(tasks).AwaitSafely();
			return Ok();
		}

		[HttpDelete(Name = "RevokeAll")]
		[NormalUser]
		[ProducesResponseType(200)]
		public async Task<IActionResult> RevokeAll()
		{
			IEnumerable<UserToken> tokens;
			var user = await this.GetCurrentUserAsync().AwaitSafely();

			tokens = this._tokens.GetByUser(user);
			var tasks = new[] {
                this._audit_log.CreateAsync(
                    this.GetCurrentRoute(), RequestMethod.HttpDelete,
                    GetRemoteAddress(), user
                ),
                this._tokens.InvalidateManyAsync(tokens)
			};

			await Task.WhenAll(tasks).AwaitSafely();
			return Ok();
		}

		private UserToken CreateUserTokenEntry(SensateUser user)
		{
			var token = new UserToken {
				UserId = user.Id,
				User = user,
				ExpiresAt = DateTime.Now.AddMinutes(this._settings.JwtRefreshExpireMinutes),
				LoginProvider = UserTokenRepository.JwtRefreshTokenProvider,
				Value = this._tokens.GenerateRefreshToken()
			};

			return token;
		}
	}
}
