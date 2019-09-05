/*
 * API key validation middleware.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.ApiCore.Middleware
{
	public class ApiKeyValidationMiddleware
	{
		private readonly ILogger<ApiKeyValidationMiddleware> _logger;
		private readonly IServiceProvider _provider;
		private readonly RequestDelegate _next;

		public ApiKeyValidationMiddleware(RequestDelegate next, IServiceProvider sp, ILogger<ApiKeyValidationMiddleware> logger)
		{
			this._next = next;
			this._logger = logger;
			this._provider = sp;
		}

		public static bool IsBanned(SensateUser user, SensateRole role)
		{
			return user.UserRoles == null || user.UserRoles.Any(r => r.RoleId == role.Id);
		}

		public static bool IsSwagger(string url)
		{
			return url.Contains("swagger");
		}

		public static async Task RespondErrorAsync(HttpContext ctx, ReplyCode code, string err, int http)
		{
			Status output = new Status {
				ErrorCode = code,
				Message = err
			};

			ctx.Response.Headers["Content-Type"] = "application/json";
			ctx.Response.StatusCode = 400;

			await ctx.Response.WriteAsync(JsonConvert.SerializeObject(output)).AwaitBackground();
		}

		public async Task Invoke(HttpContext ctx)
		{
			var query = ctx.Request.Query;

			if(IsSwagger(ctx.Request.Path)) {
				await this._next(ctx).AwaitBackground();
				return;
			}

			if(!query.TryGetValue("key", out var key)) {
				await RespondErrorAsync(ctx, ReplyCode.NotAllowed, "API key missing!", 400).AwaitBackground();
				return;
			}

			using(var scope = this._provider.CreateScope()) {
				var repo = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
				var roles = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
				var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
				var token = await repo.GetByKeyAsync(key, CancellationToken.None).AwaitBackground();

				if(token == null) {
					await RespondErrorAsync(ctx, ReplyCode.BadInput, "API key not found!", 403).AwaitBackground();
					return;
				}

				token.User = await users.GetAsync(token.UserId).AwaitBackground();
				var banned = await roles.GetByNameAsync(SensateRole.Banned).AwaitBackground();

				if(IsBanned(token.User, banned)) {
					await RespondErrorAsync(ctx, ReplyCode.Banned, "Bad API key!", 403).AwaitBackground();
					return;
				}

				if(token.Revoked) {
					await RespondErrorAsync(ctx, ReplyCode.NotAllowed, "Bad API key!", 403).AwaitBackground();
					return;
				}

				ctx.Items["ApiKey"] = token;
				await this._next(ctx).AwaitBackground();
			}

			this._logger.LogInformation($"Key: {key}");
		}
	}
}
