/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json;

namespace SensateService.Controllers
{
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class AccountsController : Controller
	{
		private readonly UserAccountSettings _settings;
		private readonly SignInManager<SensateUser> _siManager;
		private readonly UserManager<SensateUser> _manager;
		private readonly IUserRepository _users;

		public AccountsController(
			IUserRepository repo,
			SignInManager<SensateUser> manager,
			UserManager<SensateUser> userManager,
			IOptions<UserAccountSettings> options
		)
		{
			this._users = repo;
			this._siManager = manager;
			this._settings = options.Value;
			this._manager = userManager;
		}

		public async Task<object> Login([FromBody] LoginModel loginModel)
		{
			var result = await this._siManager.PasswordSignInAsync(
				loginModel.Email,
				loginModel.Password,
				false,
				false
			);

			if(result.Succeeded) {
				var user = await this._users.GetByEmailAsync(loginModel.Email);
				return this.GenerateJwtToken(loginModel.Email, user);
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

		[HttpPost]
		public async Task<object> Register([FromBody] RegisterModel register)
		{
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
				await this._siManager.SignInAsync(user, false);
				return this.GenerateJwtToken(register.Email, user);
			}

			return BadRequest();
		}

		[HttpGet("show")]
		[Authorize]
		public async Task<IActionResult> Show()
		{
			dynamic jobj;
			var user = await this._users.GetCurrentUserAsync(this.User);

			if(user == null)
				return NotFound();

			jobj = new JObject();

			jobj.FirstName = user.FirstName;
			jobj.LastName = user.LastName;
			jobj.Email = user.Email;
			jobj.PhoneNumber = user.PhoneNumber == null ? "" : user.PhoneNumber;

			return new ObjectResult(jobj);
		}

		private object GenerateJwtToken(string email, SensateUser user)
		{
			List<Claim> claims;
			JwtSecurityToken token;

			claims = new List<Claim> {
				new Claim(JwtRegisteredClaimNames.Sub, email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._settings.JwtKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.Now.AddDays(this._settings.JwtExpireDays);
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
