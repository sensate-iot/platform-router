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

using SensateIoT.Platform.Network.API.DTO;

namespace SensateIoT.Platform.Network.API.Middleware
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly JsonSerializerSettings m_settings;
		private readonly ILogger<RequestLoggingMiddleware> m_logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
		{
			this._next = next;
			this.m_settings = new JsonSerializerSettings {
				ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.None
			};

			this.m_logger = logger;
		}

		public async Task RespondErrorAsync(HttpContext ctx, string err, int http)
		{
			var response = new Response<string>();

			response.AddError(err);

			ctx.Response.Headers["Content-Type"] = "application/json";
			ctx.Response.StatusCode = http;

			await ctx.Response.WriteAsync(JsonConvert.SerializeObject(response, this.m_settings)).ConfigureAwait(false);
		}

		public async Task Invoke(HttpContext ctx)
		{
			var sw = Stopwatch.StartNew();

			try {
				await this._next(ctx).ConfigureAwait(false);
			} catch(Exception ex) {
				this.m_logger.LogError("Uncaught exception: {message}. Trace: {trace}", ex.Message, ex.StackTrace);
			}

			// TODO:: ....
		}
	}
}
