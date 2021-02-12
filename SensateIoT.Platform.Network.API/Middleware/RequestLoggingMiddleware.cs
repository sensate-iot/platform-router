/*
 * Request logging middleware.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.API.Middleware
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggingMiddleware> m_logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
		{
			this._next = next;
			this.m_logger = logger;
		}

		public async Task Invoke(HttpContext ctx)
		{
			var sw = Stopwatch.StartNew();
			string userId;

			if(ctx.Items["ApiKey"] is ApiKey token) {
				userId = token.User.ID.ToString("D");
			} else {
				userId = null;
			}

			var auditRepo = ctx.RequestServices.GetRequiredService<IAuditLogRepository>();
			await auditRepo.CreateAsync(new AuditLog {
				Address = ctx.Request.HttpContext.Connection.RemoteIpAddress,
				AuthorId = userId,
				Method = ToRequestMethod(ctx.Request.Method),
				Route = ctx.Request.Path + ctx.Request.QueryString.ToString()
			});

			await this._next(ctx).ConfigureAwait(false);

			sw.Stop();

			this.m_logger.LogInformation(
				"{method} {path} from {ip} resulted in {response} ({duration}ms)",
				ctx.Request.Method,
				ctx.Request.Path,
				ctx.Request.HttpContext.Connection.RemoteIpAddress,
				ctx.Response.StatusCode,
				sw.ElapsedMilliseconds
			);
		}

		private static RequestMethod ToRequestMethod(string method)
		{
			return method.ToUpper() switch
			{
				"GET" => RequestMethod.HttpGet,
				"POST" => RequestMethod.HttpPost,
				"PUT" => RequestMethod.HttpPut,
				"PATCH" => RequestMethod.HttpPatch,
				"DELETE" => RequestMethod.HttpDelete,
				_ => RequestMethod.Any
			};
		}
	}
}
