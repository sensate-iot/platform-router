/*
 * API key validation middleware.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SensateService.Helpers;

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

		public async Task Invoke(HttpContext ctx)
		{
			var query = ctx.Request.Query;

			await this._next(ctx).AwaitBackground();
			if(!query.TryGetValue("key", out var key)) {
				return;
			}

			this._logger.LogInformation($"Key: {key}");
		}
	}
}
