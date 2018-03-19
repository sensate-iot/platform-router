/*
 * Extension methods to initialise a new websocket service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using SensateService.Infrastructure;
using SensateService.Infrastructure.Repositories;
using SensateService.Middleware;
using SensateService.Services;

namespace SensateService.Init
{
	public static class WebSocketServiceInitExtensions
	{
		public static IApplicationBuilder MapWebSocketService(
			this IApplicationBuilder application,
			PathString path,
			WebSocketHandler handler
		)
		{
			return application.Map(path, (_app) => _app.UseMiddleware<WebSocketService>(handler));
		}

		public static IServiceCollection AddWebSocketService(this IServiceCollection services)
		{
			services.AddScoped<IWebSocketRepository, WebSocketRepository>();

			foreach(var etype in Assembly.GetEntryAssembly().ExportedTypes) {
				if(etype.GetTypeInfo().BaseType == typeof(WebSocketHandler))
					services.AddScoped(etype);
			}

			return services;
		}
	}
}
