using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SensateIoT.Platform.Router.Common.Services.Data;
using SensateIoT.Platform.Router.Common.Services.Metrics;
using SensateIoT.Platform.Router.Common.Services.Processing;
using SensateIoT.Platform.Router.Common.Settings;

namespace SensateIoT.Platform.Network.Router.Init
{
	public static class BackgroundServiceInitExtensions
	{
		public static void AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
		{
			var reload = configuration.GetValue<int>("Cache:DataReloadInterval");

			services.AddSingleton<IHostedService, DataReloadService>();
			services.AddSingleton<IHostedService, CacheTimeoutScanService>();
			services.AddSingleton<IHostedService, LiveDataReloadService>();
			services.AddSingleton<IHostedService, RoutingPublishService>();
			services.AddSingleton<IHostedService, ActuatorPublishService>();
			services.AddSingleton<IHostedService, MetricsService>();
			services.AddSingleton<IAuthorizationService, AuthorizationService>();

			services.Configure<MetricsOptions>(configuration.GetSection("HttpServer:Metrics"));
			services.Configure<DataReloadSettings>(opts => {
				opts.StartDelay = TimeSpan.FromSeconds(1);
				opts.EnableReload = configuration.GetValue<bool>("Cache:EnableReload");

				/* Default to 30 minutes for live data handler reload */
				opts.DataReloadInterval = reload == 0 ? TimeSpan.FromMinutes(30) : TimeSpan.FromSeconds(reload);
				opts.LiveDataReloadInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("Cache:LiveDataReloadInterval"));
				opts.TimeoutScanInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("Cache:TimeoutScanInterval"));
			});
		}
	}
}
