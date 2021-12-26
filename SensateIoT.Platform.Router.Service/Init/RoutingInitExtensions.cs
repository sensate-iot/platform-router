using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Caching.Routing;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Local;
using SensateIoT.Platform.Router.Common.Collections.Remote;
using SensateIoT.Platform.Router.Common.Services.Processing;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.DataAccess.Abstract;
using SensateIoT.Platform.Router.DataAccess.Repositories;

namespace SensateIoT.Platform.Router.Service.Init
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

			services.Configure<RoutingQueueSettings>(s => {
				s.InternalInterval = TimeSpan.FromMilliseconds(configuration.GetValue<int>("Routing:InternalPublishInterval"));
				s.PublicInterval = TimeSpan.FromMilliseconds(configuration.GetValue<int>("Routing:PublicPublishInterval"));
				s.ActuatorTopicFormat = configuration.GetValue<string>("Routing:ActuatorTopicFormat");
				s.DequeueBatchSize = configuration.GetValue<int>("Routing:DequeueBatchSize");
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