using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SensateIoT.Platform.Network.API.DTO;

namespace SensateIoT.Platform.Network.API.Middleware
{
	public class JsonErrorHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly JsonSerializerSettings m_settings;

		public JsonErrorHandlerMiddleware(RequestDelegate next)
		{
			this._next = next;
			this.m_settings = new JsonSerializerSettings {
				ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.None
			};
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
			try {
				await this._next(ctx).ConfigureAwait(false);
			} catch(JsonSerializationException ex) {
				await this.RespondErrorAsync(ctx, ex.Message, 400);
			} catch(FormatException ex) {
				await this.RespondErrorAsync(ctx, ex.Message, 400);
			}
		}
	}
}