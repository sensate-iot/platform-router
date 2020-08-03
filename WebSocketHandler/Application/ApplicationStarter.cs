/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Init;
using SensateService.Config;
using SensateService.Init;
using SensateService.WebSocketHandler.Handlers;

namespace SensateService.WebSocketHandler.Application
{
	public class ApplicationStarter
	{
		private readonly IConfiguration Configuration;

		public ApplicationStarter(IConfiguration config)
		{
			this.Configuration = config;
		}

		private static bool IsDevelopment()
		{
#if DEBUG
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			return env == "Development";
#else
			return false;
#endif

		}

		public void ConfigureServices(IServiceCollection services)
		{
			var auth = new AuthenticationConfig();
			var sys = new SystemConfig();

			this.Configuration.GetSection("System").Bind(sys);
			this.Configuration.GetSection("Authentication").Bind(auth);

			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });

			services.AddIdentityFramwork(auth);
			services.AddHashAlgorihms();
			services.AddReverseProxy(sys);
			services.AddAuthorizationProxy(sys);

			services.AddWebSocketHandler<WebSocketMessageHandler>();
			services.AddWebSocketHandler<WebSocketMeasurementHandler>();
			services.AddControllers().AddNewtonsoftJson();

			services.AddLogging((builder) => {
				builder.AddConsole();

				if(IsDevelopment()) {
					builder.AddDebug();
				}
			});
		}

		public void Configure(IApplicationBuilder app, IServiceProvider sp)
		{
			var cache = new CacheConfig();

			app.UseForwardedHeaders();
			app.UseRouting();
			app.UseWebSockets();

			this.Configuration.GetSection("Cache").Bind(cache);

			app.MapWebSocketService("/ingress/v1/message", sp.GetService<WebSocketMessageHandler>());
			app.MapWebSocketService("/ingress/v1/measurement", sp.GetService<WebSocketMeasurementHandler>());

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}
	}
}
