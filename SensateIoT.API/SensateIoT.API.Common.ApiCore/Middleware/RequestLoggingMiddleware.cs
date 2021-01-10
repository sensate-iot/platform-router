/*
 * ASP.NET middleware to log requests and responses.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.ApiCore.Middleware
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IServiceProvider _provider;
		private readonly ILogger<RequestLoggingMiddleware> _logger;
		private readonly JsonSerializerSettings m_settings;

		public RequestLoggingMiddleware(RequestDelegate next, IServiceProvider sp, ILogger<RequestLoggingMiddleware> logger)
		{
			this._next = next;
			this._provider = sp;
			this._logger = logger;
			this.m_settings = new JsonSerializerSettings {
				ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.None
			};
		}

		private static RequestMethod ToRequestMethod(string method)
		{
			return method.ToUpper(CultureInfo.InvariantCulture) switch
			{
				"GET" => RequestMethod.HttpGet,
				"POST" => RequestMethod.HttpPost,
				"PUT" => RequestMethod.HttpPut,
				"PATCH" => RequestMethod.HttpPatch,
				"DELETE" => RequestMethod.HttpDelete,
				_ => RequestMethod.Any
			};
		}

		private async Task RespondErrorAsync(HttpContext ctx, ReplyCode code, string err, int http)
		{
			var output = new Status {
				ErrorCode = code,
				Message = err
			};

			ctx.Response.Headers["Content-Type"] = "application/json";
			ctx.Response.StatusCode = http;

			await ctx.Response.WriteAsync(JsonConvert.SerializeObject(output, this.m_settings)).AwaitBackground();
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Reviewed.")]
		[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Reviewed.")]
		public async Task Invoke([NotNull] HttpContext ctx)
		{
			AuditLog log;
			SensateUser user;

			try {
				if(ctx == null) {
					throw new NullReferenceException("Unable to handle NULL context!");
				}

				var sw = Stopwatch.StartNew();
				await this._next(ctx).AwaitBackground();
				sw.Stop();

				using var scope = this._provider.CreateScope();
				var logs = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
				var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

				this._logger.LogInformation(
					"{method} {path} from {ip} resulted in {response} ({duration}ms)",
					ctx.Request.Method,
					ctx.Request.Path,
					ctx.Request.HttpContext.Connection.RemoteIpAddress,
					ctx.Response?.StatusCode,
					sw.ElapsedMilliseconds
				);

				if(ctx.Items["ApiKey"] is SensateApiKey token) {
					user = token.User;
				} else {
					user = await users.GetByClaimsPrincipleAsync(ctx.User).AwaitBackground();
				}

				log = new AuditLog {
					Timestamp = DateTime.Now,
					Method = ToRequestMethod(ctx.Request.Method),
					Address = ctx.Request.HttpContext.Connection.RemoteIpAddress,
					AuthorId = user?.Id,
					Route = ctx.Request.Path + ctx.Request.QueryString.ToString()
				};

				await logs.CreateAsync(log, CancellationToken.None).AwaitBackground();
			} catch(FormatException ex) {
				this._logger.LogInformation(ex, "Invalid format detected!");
				await this.RespondErrorAsync(ctx, ReplyCode.BadInput, "Invalid input supplied!", 422).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex, "Unknown error occurred!");

				await this.RespondErrorAsync(ctx, ReplyCode.UnknownError, "Bad request!", 500).AwaitBackground();
			}
		}
	}
}
