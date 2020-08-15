/*
 * Init extensions for the identity framework.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Models;
using SensateService.Settings;

namespace SensateService.ApiCore.Init
{
	public static class IdentityInitExtensions
	{
		public static IServiceCollection AddIdentityFramwork(this IServiceCollection services, AuthenticationConfig auth)
		{
			services.Configure<UserAccountSettings>(options => {
				options.JwtKey = auth.JwtKey;
				options.JwtIssuer = auth.JwtIssuer;
				options.JwtExpireMinutes = auth.JwtExpireMinutes;
				options.JwtRefreshExpireMinutes = auth.JwtRefreshExpireMinutes;
				options.PublicUrl = auth.PublicUrl;
				options.Scheme = auth.Scheme;
			});

			services.AddIdentity<SensateUser, SensateRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

			if(auth.PrimaryAuthHost) {
				services.AddDataProtection().PersistKeysToDbContext<SensateSqlContext>();
			} else {
				services.AddDataProtection()
					.PersistKeysToDbContext<SensateSqlContext>()
					.DisableAutomaticKeyGeneration();
			}

			services.Configure<IdentityOptions>(options => {
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 8;
				options.Password.RequiredUniqueChars = 5;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				options.User.RequireUniqueEmail = true;
			});

			services.AddAuthentication(options => {
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(cfg => {
				cfg.RequireHttpsMetadata = false;
				cfg.SaveToken = true;
				cfg.TokenValidationParameters = new TokenValidationParameters {
					ValidIssuer = auth.JwtIssuer,
					ValidAudience = auth.JwtIssuer,
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(auth.JwtKey)
					),
					ClockSkew = TimeSpan.Zero
				};
			});

			return services;
		}
	}
}