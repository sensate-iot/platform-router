/*
 * ASP.NET middleware to log requests and responses.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.ApiCore.Middleware
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IServiceProvider _provider;
		private readonly ILogger<RequestLoggingMiddleware> _logger;

		public RequestLoggingMiddleware(RequestDelegate next, IServiceProvider sp, ILogger<RequestLoggingMiddleware> logger)
		{
			this._next = next;
			this._provider = sp;
			this._logger = logger;
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

		public async Task Invoke(HttpContext ctx)
		{
			AuditLog log;
			SensateUser user = null;

			var sw = Stopwatch.StartNew();
			await this._next(ctx).AwaitBackground();
			sw.Stop();

			using(var scope = this._provider.CreateScope()) {
				var logs = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
				var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

				this._logger.LogInformation($"{ctx.Request.Method}: {ctx.Request.Path} {Environment.NewLine}" +
				                            $"Result: HTTP {ctx.Response?.StatusCode} {Environment.NewLine}"  +
				                            $"Client IP: {ctx.Request.HttpContext.Connection.RemoteIpAddress.ToString()}");

				if(ctx.User != null)
					user = await users.GetByClaimsPrincipleAsync(ctx.User).AwaitBackground();

				log = new AuditLog {
					Timestamp = DateTime.Now,
					Method = ToRequestMethod(ctx.Request.Method),
					Address = ctx.Request.HttpContext.Connection.RemoteIpAddress,
					AuthorId = user?.Id,
					Route = ctx.Request.Path
				};

				await logs.CreateAsync(log, CancellationToken.None).AwaitBackground();
			}

			this._logger.LogInformation($"Request completed in {sw.ElapsedMilliseconds}ms");
		}
	}
}
