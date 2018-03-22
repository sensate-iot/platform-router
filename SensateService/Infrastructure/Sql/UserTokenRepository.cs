/*
 * Repository for the SensateUserToken table.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SensateService.Controllers;

namespace SensateService.Infrastructure.Sql
{
	public class UserTokenRepository :
		AbstractSqlRepository<Tuple<SensateUser, string>, UserToken>, IUserTokenRepository
	{
		private Random _rng;
		private const int JwtRefreshTokenLength = 64;
		public const string JwtRefreshTokenProvider = "JWTrt";
		public const string JwtTokenProvider = "JWT";

		public UserTokenRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random();
		}

		public override void Create(UserToken obj)
		{
			var asyncResult = this.CreateAsync(obj);
			asyncResult.Wait();
		}

		public override async Task CreateAsync(UserToken obj)
		{
			if(obj.Value == null && obj.LoginProvider == JwtRefreshTokenProvider)
				obj.Value = this.GenerateRefreshToken();
			else if(obj.Value == null)
				throw new DatabaseException("User token must have a value!");

			await this.Data.AddAsync(obj);
			await this.CommitAsync(obj);
		}


		public override UserToken GetById(Tuple<SensateUser, string> id) => this.GetById(id.Item1, id.Item2);

		public UserToken GetById(SensateUser user, string value)
		{
			return this.Data.FirstOrDefault(
				x => x.UserId == user.Id && x.Value == value
			);
		}

		public IEnumerable<UserToken> GetByUser(SensateUser user)
		{
			IEnumerable<UserToken> data;

			data = from token in this.Data
				   where token.UserId == user.Id
				   select token;

			return data;
		}


		public override void Update(UserToken obj)
		{
			throw new NotAllowedException("Unable to update user token!");
		}

		public override void Delete(Tuple<SensateUser, string> id)
		{
			throw new NotAllowedException("Unable to delete user token!");
		}

		public override Task DeleteAsync(Tuple<SensateUser, string> id)
		{
			throw new NotAllowedException("Unable to delete user token!");
		}

		public void InvalidateToken(UserToken token)
		{
			this.StartUpdate(token);
			token.Valid = false;
			this.EndUpdate();
		}

		public async Task InvalidateTokenAsync(UserToken token)
		{
			this.StartUpdate(token);
			token.Valid = false;
			await this.EndUpdateAsync();
		}

		public void InvalidateToken(SensateUser user, string value)
		{
			var token = this.GetById(user, value);

			if(token == null)
				return;

			this.InvalidateToken(token);
		}

		public async Task InvalidateTokenAsync(SensateUser user, string value)
		{
			var token = this.GetById(user, value);

			if(token == null)
				return;

			await this.InvalidateTokenAsync(token);
		}

		public string GenerateJwtToken(SensateUser user, IEnumerable<string> roles, UserAccountSettings settings)
		{
			List<Claim> claims;
			JwtSecurityToken token;

			claims = new List<Claim> {
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id)
			};

			roles.ToList().ForEach(x => {
				claims.Add(new Claim(ClaimTypes.Role, x));
			});

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.Now.AddMinutes(settings.JwtExpireMinutes);
			token = new JwtSecurityToken(
				issuer: settings.JwtIssuer,
				audience: settings.JwtIssuer,
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public string GenerateRefreshToken()
		{
			return this._rng.NextString(JwtRefreshTokenLength);
		}

		public async Task InvalidateManyAsync(IEnumerable<UserToken> tokens)
		{
			List<UserToken> _tokens;

			_tokens = tokens.ToList();
			_tokens.ForEach(x => {
				this.StartUpdate(x);
				x.Valid = false;
			});

			await this.CommitAsync();
		}
	}
}
