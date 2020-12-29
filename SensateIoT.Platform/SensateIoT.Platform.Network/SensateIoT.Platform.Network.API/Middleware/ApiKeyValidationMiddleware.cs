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

using SensateIoT.Platform.Network.API.Constants;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.API.Middleware
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

		public static bool IsBanned([NotNull] User user)
		{
			var banned = UserRoles.Banned.ToUpperInvariant();
			return user.UserRoles == null || user.UserRoles.Any(r => r.Equals(banned));
		}

		public static bool IsSwagger(string url)
		{
			return url.Contains("swagger", StringComparison.OrdinalIgnoreCase);
		}

		public async Task RespondErrorAsync([NotNull] HttpContext ctx, string err, int http)
		{
			var response = new Response<string>();

			response.AddError(err);

			ctx.Response.Headers["Content-Type"] = "application/json";
			ctx.Response.StatusCode = http;

			await ctx.Response.WriteAsync(JsonConvert.SerializeObject(response, this.m_settings)).ConfigureAwait(false);
		}

		public async Task Invoke([NotNull] HttpContext ctx)
		{
			var query = ctx.Request.Query;
			string key;

			if(IsSwagger(ctx.Request.Path)) {
				await this._next(ctx).ConfigureAwait(false);
				return;
			}

			if(query.TryGetValue("key", out var sv)) {
				key = sv;
			} else if(ctx.Request.Headers.TryGetValue("X-ApiKey", out sv)) {
				key = sv;
			} else {
				await this.RespondErrorAsync(ctx, "API key missing!", 400).ConfigureAwait(false);
				return;
			}

			using var scope = this._provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
			var users = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
			var token = await repo.GetAsync(key, CancellationToken.None).ConfigureAwait(false);

			if(token == null) {
				await this.RespondErrorAsync(ctx, "API key not found!", 401).ConfigureAwait(false);
				return;
			}

			await repo.IncrementRequestCountAsync(token.Key).ConfigureAwait(false);

			token.User = await users.GetAccountAsync(token.UserId).ConfigureAwait(false);

			if(IsBanned(token.User)) {
				await this.RespondErrorAsync(ctx, "Bad API key!", 403).ConfigureAwait(false);
				return;
			}

			if(token.Revoked) {
				await this.RespondErrorAsync(ctx, "Bad API key!", 403).ConfigureAwait(false);
				return;
			}

			if(token.User.BillingLockout) {
				await this.RespondErrorAsync(ctx, "Billing lockout!", 402).ConfigureAwait(false);
				return;
			}

			ctx.Items["ApiKey"] = token;
			await this._next(ctx).ConfigureAwait(false);
		}
	}
}
