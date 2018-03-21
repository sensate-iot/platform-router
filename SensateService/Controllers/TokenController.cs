/*
 * Authentication token controller.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Sql;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.Controllers
{
	[Route(template: "[controller]")]
	public class TokensController : AbstractController
	{
		private readonly ISensateUserTokenRepository _tokens;
		private readonly SignInManager<SensateUser> _signin_manager;
		private readonly UserAccountSettings _settings;

		public TokensController(
			ISensateUserTokenRepository tokens,
			IOptions<UserAccountSettings> options,
			IUserRepository users,
			SignInManager<SensateUser> signInManager
		) : base(users)
		{
			this._tokens = tokens;
			this._signin_manager = signInManager;
			this._settings = options.Value;
		}

		[HttpPost("request")]
		public async Task<ActionResult> RequestToken([FromBody] Login login)
		{
			var user = await this._users.GetByEmailAsync(login.Email);
			bool result;
			Microsoft.AspNetCore.Identity.SignInResult signInResult;
			SensateUserToken token;

			if(user == null)
				return NotFound();

			result = await this._signin_manager.CanSignInAsync(user);

			if(!result) {
				var status = new Status();
				status.ErrorCode = 401;
				status.Message = "Not allowed to sign in!";
				return new BadRequestObjectResult(status);
			}

			signInResult = await this._signin_manager.PasswordSignInAsync(user, login.Password, false, false);

			if(!signInResult.Succeeded)
				return new UnauthorizedResult();

			token = new SensateUserToken {
				UserId = user.Id,
				User = user,
				ExpiresAt = DateTime.Now.AddMinutes(this._settings.JwtRefreshExpireMinutes),
				LoginProvider = SensateUserTokenRepository.JwtRefreshTokenProvider,
				Value = this._tokens.GenerateRefreshToken()
			};

			await this._tokens.CreateAsync(token);

			var roles = this._users.GetRoles(user);
			var reply = new TokenRequestReply {
				RefreshToken = token.Value,
				ExpiresInMinutes = this._settings.JwtRefreshExpireMinutes,
				JwtToken = this._tokens.GenerateJwtToken(user, roles, _settings)
			};

			return new OkObjectResult(reply);
		}

		[HttpPost("refresh")]
		public async Task<ActionResult> RefreshToken([FromBody] RefreshLogin login)
		{
			await Task.CompletedTask;
			return this.NotFound();
		}
	}
}
