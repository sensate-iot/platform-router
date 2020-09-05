/*
 * API key validation middleware.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;

namespace SensateService.ApiCore.Middleware
{
	public class ApiKeyValidationMiddleware
	{
		private readonly IServiceProvider _provider;
		private readonly RequestDelegate _next;
		private readonly JsonSerializerSettings m_settings;

		public ApiKeyValidationMiddleware(RequestDelegate next, IServiceProvider sp)
		{
			this._next = next;
			this._provider = sp;
			this.m_settings = new JsonSerializerSettings {
				ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.None
			};
		}

		public static bool IsBanned([NotNull] SensateUser user, [NotNull] SensateRole role)
		{
			return user.UserRoles == null || user.UserRoles.Any(r => r.RoleId == role.Id);
		}

		public static bool IsSwagger(string url)
		{
			return url.Contains("swagger", StringComparison.OrdinalIgnoreCase);
		}

		public async Task RespondErrorAsync([NotNull] HttpContext ctx, ReplyCode code, string err, int http)
		{
			var output = new Status {
				ErrorCode = code,
				Message = err
			};

			ctx.Response.Headers["Content-Type"] = "application/json";
			ctx.Response.StatusCode = http;

			await ctx.Response.WriteAsync(JsonConvert.SerializeObject(output, this.m_settings)).AwaitBackground();
		}

		public async Task Invoke([NotNull] HttpContext ctx)
		{
			var query = ctx.Request.Query;
			string key;

			if(IsSwagger(ctx.Request.Path)) {
				await this._next(ctx).AwaitBackground();
				return;
			}

			if(query.TryGetValue("key", out var sv)) {
				key = sv;
			} else if(ctx.Request.Headers.TryGetValue("X-ApiKey", out sv)) {
				key = sv;
			} else {
				await this.RespondErrorAsync(ctx, ReplyCode.NotAllowed, "API key missing!", 400).AwaitBackground();
				return;
			}

			using var scope = this._provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
			var roles = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
			var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
			var token = await repo.GetAsync(key, CancellationToken.None).AwaitBackground();

			if(token == null) {
				await this.RespondErrorAsync(ctx, ReplyCode.BadInput, "API key not found!", 401).AwaitBackground();
				return;
			}

			await repo.IncrementRequestCountAsync(token).AwaitBackground();

			token.User = await users.GetAsync(token.UserId, false).AwaitBackground();
			var banned = await roles.GetByNameAsync(SensateRole.Banned).AwaitBackground();

			if(IsBanned(token.User, banned)) {
				await this.RespondErrorAsync(ctx, ReplyCode.Banned, "Bad API key!", 403).AwaitBackground();
				return;
			}

			if(token.Revoked) {
				await this.RespondErrorAsync(ctx, ReplyCode.NotAllowed, "Bad API key!", 403).AwaitBackground();
				return;
			}

			if(token.User.BillingLockout) {
				await this.RespondErrorAsync(ctx, ReplyCode.BillingLockout, "Billing lockout!", 402).AwaitBackground();
				return;
			}

			ctx.Items["ApiKey"] = token;
			await this._next(ctx).AwaitBackground();
		}
	}
}
