using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Routing;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Local;
using SensateIoT.Platform.Network.Common.Collections.Remote;
using SensateIoT.Platform.Network.Common.Services.Processing;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;

namespace SensateIoT.Platform.Network.Router.Init
{
	public static class RoutingInitExtensions
	{
		public static void AddRoutingServices(this IServiceCollection services, IConfiguration configuration)
		{
			AddRoutingService(services, configuration);
			AddQueues(services, configuration);

			// Data repository's
			services.AddScoped<IRoutingRepository, RoutingRepository>();
			services.AddScoped<ILiveDataHandlerRepository, LiveDataHandlerRepository>();
		}

		private static void AddRoutingService(IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IRoutingCache, RoutingCache>();
			services.AddSingleton<IHostedService, RoutingService>();

			services.Configure<RoutingPublishSettings>(s => {
				s.InternalInterval = TimeSpan.FromMilliseconds(configuration.GetValue<int>("Routing:InternalPublishInterval"));
				s.PublicInterval = TimeSpan.FromMilliseconds(configuration.GetValue<int>("Routing:PublicPublishInterval"));
				s.ActuatorTopicFormat = configuration.GetValue<string>("Routing:ActuatorTopicFormat");
			});
		}

		private static void AddQueues(IServiceCollection services, IConfiguration configuration)
		{
			// Routing queues
			services.AddSingleton<IQueue<IPlatformMessage>, MessageQueue>();
			services.AddSingleton<IRemoteNetworkEventQueue, RemoteNetworkEventQueue>();
			services.AddSingleton<IInternalRemoteQueue, InternalMqttQueue>();
			services.AddSingleton<IPublicRemoteQueue, PublicMqttQueue>();
			services.AddSingleton<IRemoteStorageQueue, RemoteStorageQueue>();

			services.Configure<QueueSettings>(s => {
				s.LiveDataQueueTemplate = configuration.GetValue<string>("Routing:LiveDataTopic");
				s.TriggerQueueTemplate = configuration.GetValue<string>("Routing:TriggerTopic");
				s.MessageStorageQueueTopic = configuration.GetValue<string>("Routing:MessageStorageQueueTopic");
				s.MeasurementStorageQueueTopic = configuration.GetValue<string>("Routing:MeasurementStorageQueueTopic");
				s.NetworkEventQueueTopic = configuration.GetValue<string>("Routing:NetworkEventQueueTopic");
			});
		}
	}
}