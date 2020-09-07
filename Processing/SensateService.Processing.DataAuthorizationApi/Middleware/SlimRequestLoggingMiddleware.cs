/*
 * Request logging middleware.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Helpers;

namespace SensateService.Processing.DataAuthorizationApi.Middleware
{
	public class SlimRequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExecutionTimeMeasurementMiddleware> _logger;
		private readonly JsonSerializerSettings m_settings;

		private const string ExecutionTimeHeader = "X-ExecutionTime";

		public SlimRequestLoggingMiddleware(RequestDelegate next, ILogger<ExecutionTimeMeasurementMiddleware> logger)
		{
			this._next = next;
			this._logger = logger;
			this.m_settings = new JsonSerializerSettings {
				ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.None
			};
		}

		public async Task RespondErrorAsync(HttpContext ctx, ReplyCode code, string err, int http)
		{
			var output = new Status {
				ErrorCode = code,
				Message = err
			};

			ctx.Response.Headers["Content-Type"] = "application/json";
			ctx.Response.StatusCode = http;

			await ctx.Response.WriteAsync(JsonConvert.SerializeObject(output, this.m_settings)).AwaitBackground();
		}

		public async Task Invoke(HttpContext ctx)
		{
			try {
				await this._next(ctx).AwaitBackground();
			} catch(FormatException ex) {
				this._logger.LogInformation(ex, "Incorrectly formatted request: {message}" + Environment.NewLine + "{data}", ex.Message, ex.Data);
				await this.RespondErrorAsync(ctx, ReplyCode.BadInput, "Invalid input supplied!", 422).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex, "Unable to complete request: {message}", ex.InnerException?.Message);
				await this.RespondErrorAsync(ctx, ReplyCode.UnknownError, "Bad request!", 500).AwaitBackground();
			}
		}
	}
}