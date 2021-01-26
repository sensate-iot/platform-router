/*
 * Authentication token controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SensateIoT.API.Common.ApiCore.Attributes;
using SensateIoT.API.Common.ApiCore.Controllers;
using SensateIoT.API.Common.Config.Settings;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Core.Infrastructure.Sql;
using SensateIoT.API.Common.Data.Dto.Json.In;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.IdentityData.Enums;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.AuthApi.Controllers
{
	[Produces("application/json")]
	[Route("auth/v1/[controller]")]
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
		[ProducesResponseType(typeof(TokenRequestReply), 200)]
		[ProducesResponseType(401)]
		[ProducesResponseType(404)]
		public async Task<ActionResult> RequestToken([FromBody] Login login)
		{
			var user = await this._users.GetByEmailAsync(login.Email).AwaitBackground();
			bool result;
			Microsoft.AspNetCore.Identity.SignInResult signInResult;
			AuthUserToken token;

			if(user == null)
				return this.NotFound();

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
				return this.Unauthorized();
			}

			token = this.CreateAuthUserTokenEntry(user);
			await this._tokens.CreateAsync(token);

			var roles = this._users.GetRoles(user);
			var key = await this.CreateSystemApiKeyAsync(user).AwaitBackground();

			var reply = new TokenRequestReply {
				RefreshToken = token.Value,
				ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes,
				JwtToken = this._tokens.GenerateJwtToken(user, roles, this._settings),
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
		[ProducesResponseType(403)]
		[ProducesResponseType(401)]
		public async Task<ActionResult> RefreshToken([FromBody] RefreshLogin login)
		{
			var user = await this._users.GetByEmailAsync(login.Email).AwaitBackground();
			TokenRequestReply reply;
			AuthUserToken token;
			bool banned;

			if(user == null) {
				return this.Forbid();
			}

			banned = await this._users.IsBanned(user);
			token = this._tokens.GetById(user, login.RefreshToken);

			if(token == null || !token.Valid || banned) {
				return this.Forbid();
			}

			if(token.ExpiresAt < DateTime.Now) {
				await this._tokens.InvalidateTokenAsync(token).AwaitBackground();
				return this.Forbid();
			}

			reply = new TokenRequestReply();
			var newToken = this.CreateAuthUserTokenEntry(user);

			var roles = await this._users.GetRolesAsync(user).AwaitBackground();
			await this._tokens.CreateAsync(newToken).AwaitBackground();
			await this._tokens.InvalidateTokenAsync(token).AwaitBackground();

			reply.RefreshToken = newToken.Value;
			reply.JwtExpiresInMinutes = this._settings.JwtRefreshExpireMinutes;
			reply.ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes;
			reply.JwtToken = this._tokens.GenerateJwtToken(user, roles, this._settings);

			return new OkObjectResult(reply);
		}

		[NormalUser]
		[HttpDelete("revoke/{token}", Name = "RevokeToken")]
		[ProducesResponseType(204)]
		[ProducesResponseType(typeof(Status), 404)]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(403)]
		[ProducesResponseType(401)]
		public async Task<IActionResult> Revoke(string token)
		{
			AuthUserToken authToken;
			var user = await this.GetCurrentUserAsync().AwaitBackground();

			if(user == null) {
				return this.Forbid();
			}

			if(string.IsNullOrEmpty(token)) {
				return InvalidInputResult("Token not found!");
			}

			authToken = this._tokens.GetById(user, token);

			if(authToken == null)
				return NotFoundInputResult("Token not found!");

			if(!authToken.Valid)
				return InvalidInputResult("Token already invalid!");

			await this._tokens.InvalidateTokenAsync(authToken);
			return this.NoContent();
		}

		[NormalUser]
		[HttpDelete("revoke-all", Name = "RevokeAll")]
		[ProducesResponseType(204)]
		[ProducesResponseType(403)]
		[ProducesResponseType(401)]
		public async Task<IActionResult> RevokeAll()
		{
			IEnumerable<AuthUserToken> tokens;
			var user = await this.GetCurrentUserAsync().AwaitBackground();

			tokens = await this._tokens.GetByUserAsync(user).AwaitBackground();
			await this._tokens.InvalidateManyAsync(tokens);

			return this.NoContent();
		}

		private AuthUserToken CreateAuthUserTokenEntry(SensateUser user)
		{
			var token = new AuthUserToken {
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
