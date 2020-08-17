/*
 * ASP.NET middleware to log requests and responses.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;

namespace SensateService.ApiCore.Middleware
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
			switch(method.ToUpper()) {
			case "GET":
				return RequestMethod.HttpGet;

			case "POST":
				return RequestMethod.HttpPost;

			case "PUT":
				return RequestMethod.HttpPut;

			case "PATCH":
				return RequestMethod.HttpPatch;

			case "DELETE":
				return RequestMethod.HttpDelete;

			default:
				return RequestMethod.Any;
			}

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
			AuditLog log;
			SensateUser user;

			try {
				var sw = Stopwatch.StartNew();
				await this._next(ctx).AwaitBackground();
				sw.Stop();

				using(var scope = this._provider.CreateScope()) {
					var logs = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
					var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

					this._logger.LogInformation($"{ctx.Request.Method}: {ctx.Request.Path} {Environment.NewLine}");
					this._logger.LogInformation($"Result: HTTP {ctx.Response?.StatusCode}");
					this._logger.LogInformation($"Client IP: {ctx.Request.HttpContext.Connection.RemoteIpAddress}");

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
				}

				this._logger.LogInformation($"Request completed in {sw.ElapsedMilliseconds}ms");
			} catch(FormatException ex) {
				this._logger.LogInformation($"Unable to complete request: {ex.Message}");
				this._logger.LogDebug(ex, "Invalid format detected!");

				await this.RespondErrorAsync(ctx, ReplyCode.BadInput, "Invalid input supplied!", 422).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogInformation($"Unable to complete request: {ex.Message}");
				this._logger.LogDebug(ex, "Unknown error occurred!");

				await this.RespondErrorAsync(ctx, ReplyCode.UnknownError, "Bad request!", 500).AwaitBackground();
			}
		}
	}
}
