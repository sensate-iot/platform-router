/*
 * Authentication token controller.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Sql;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class TokensController : AbstractController
	{
		private readonly IUserTokenRepository _tokens;
		private readonly SignInManager<SensateUser> _signin_manager;
		private readonly UserAccountSettings _settings;
		private readonly IApiKeyRepository _keys;

		public TokensController(
			IUserTokenRepository tokens,
			IOptions<UserAccountSettings> options,
			IUserRepository users,
			SignInManager<SensateUser> signInManager,
			IApiKeyRepository keys,
			IHttpContextAccessor ctx
		) : base(users, ctx)
		{
			this._tokens = tokens;
			this._signin_manager = signInManager;
			this._settings = options.Value;
			this._keys = keys;
		}

		[HttpPost("request")]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(typeof(TokenRequestReply), 400)]
		public async Task<ActionResult> RequestToken([FromBody] Login login)
		{
			var user = await this._users.GetByEmailAsync(login.Email).AwaitBackground();
			bool result;
			Microsoft.AspNetCore.Identity.SignInResult signInResult;
			UserToken token;

			if(user == null)
				return NotFound();

			result = await this._signin_manager.CanSignInAsync(user);
			var banned = await this._users.IsBanned(user);

			if(!result || banned) {
				var status = new Status {
					ErrorCode = ReplyCode.NotAllowed,
					Message = "Not allowed to sign in!"
				};

				if(banned)
					status.ErrorCode = ReplyCode.Banned;

				return new BadRequestObjectResult(status);
			}

			signInResult = await this._signin_manager.PasswordSignInAsync(user, login.Password, false, false);

			if(!signInResult.Succeeded) {
				return new UnauthorizedResult();
			}

			token = this.CreateUserTokenEntry(user);
			await this._tokens.CreateAsync(token);

			var roles = this._users.GetRoles(user);
			var key = await this.CreateSystemApiKeyAsync(user).AwaitBackground();

			var reply = new TokenRequestReply {
				RefreshToken = token.Value,
				ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes,
				JwtToken = this._tokens.GenerateJwtToken(user, roles, _settings),
				JwtExpiresInMinutes = this._settings.JwtExpireMinutes,
				SystemApiKey = key.ApiKey
			};

			return new OkObjectResult(reply);
		}

		private async Task<SensateApiKey> CreateSystemApiKeyAsync(SensateUser user)
		{
			SensateApiKey key;

			key = new SensateApiKey {
				Id = Guid.NewGuid().ToString(),
				User = user,
				UserId = user.Id,
				Type = ApiKeyType.SystemKey,
				Revoked = false,
				CreatedOn = DateTime.Now.ToUniversalTime(),
				ReadOnly = false,
				Name = Guid.NewGuid().ToString()
			};

			await this._keys.CreateAsync(key, CancellationToken.None).AwaitBackground();
			return key;
		}

		[HttpPost("refresh")]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(typeof(TokenRequestReply), 400)]
		public async Task<ActionResult> RefreshToken([FromBody] RefreshLogin login)
		{
			var user = await this._users.GetByEmailAsync(login.Email).AwaitBackground();
			TokenRequestReply reply;
			UserToken token;
			bool banned;

			if(user == null)
				return Forbid();

			banned = await this._users.IsBanned(user);
			token = this._tokens.GetById(user, login.RefreshToken);

			if(token == null || !token.Valid || banned) {
				return Forbid();
			}

			if(token.ExpiresAt < DateTime.Now) {
				await this._tokens.InvalidateTokenAsync(token).AwaitBackground();
				return Forbid();
			}

			reply = new TokenRequestReply();
			var newToken = this.CreateUserTokenEntry(user);

			var roles = await this._users.GetRolesAsync(user).AwaitBackground();
			await this._tokens.CreateAsync(newToken).AwaitBackground();
			await this._tokens.InvalidateTokenAsync(token).AwaitBackground();

			reply.RefreshToken = newToken.Value;
			reply.ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes;
			reply.JwtToken = this._tokens.GenerateJwtToken( user, roles, this._settings );

			return new OkObjectResult(reply);
		}

		[HttpDelete("revoke/{token}", Name = "RevokeToken")]
		[NormalUser]
		[ProducesResponseType(typeof(Status), 404)]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Revoke(string token)
		{
			UserToken authToken;
			var user = await this.GetCurrentUserAsync().AwaitBackground();

			if(user == null)
				return Forbid();

			if(String.IsNullOrEmpty(token))
				return InvalidInputResult("Token not found!");

			authToken = this._tokens.GetById(user, token);

			if(authToken == null)
				return this.NotFoundInputResult("Token not found!");

			if(!authToken.Valid)
				return this.InvalidInputResult("Token already invalid!");

			await this._tokens.InvalidateTokenAsync(authToken);
			return Ok();
		}

		[HttpDelete("revoke-all", Name = "RevokeAll")]
		[NormalUser]
		[ProducesResponseType(200)]
		public async Task<IActionResult> RevokeAll()
		{
			IEnumerable<UserToken> tokens;
			var user = await this.GetCurrentUserAsync().AwaitBackground();

			tokens = this._tokens.GetByUser(user);
			await this._tokens.InvalidateManyAsync(tokens);

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
