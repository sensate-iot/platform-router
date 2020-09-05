/*
 * Middleware to measure the execution time of a request.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using SensateService.Helpers;

namespace SensateService.Processing.DataAuthorizationApi.Middleware
{
	public class ExecutionTimeMeasurementMiddleware
	{
		private readonly RequestDelegate _next;

		private const string ExecutionTimeHeader = "X-ExecutionTime";

		public ExecutionTimeMeasurementMiddleware(RequestDelegate next)
		{
			this._next = next;
		}

		public async Task Invoke(HttpContext ctx)
		{
			var sw = Stopwatch.StartNew();

			ctx.Response.OnStarting(state => {
				var httpCtx = state as HttpContext;

				httpCtx?.Response.Headers.Add(ExecutionTimeHeader, new[] {sw.ElapsedMilliseconds.ToString()});
				return Task.CompletedTask;
			}, ctx);

			await this._next(ctx).AwaitBackground();
			sw.Stop();
		}
	}
}