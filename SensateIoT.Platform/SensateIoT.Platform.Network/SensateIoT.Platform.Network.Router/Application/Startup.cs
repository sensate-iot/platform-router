/*
 * Router service startup.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Local;
using SensateIoT.Platform.Network.Common.Collections.Remote;
using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Common.Services.Data;
using SensateIoT.Platform.Network.Common.Services.Processing;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.Router.Config;
using SensateIoT.Platform.Network.Router.MQTT;
using SensateIoT.Platform.Network.Router.Services;

namespace SensateIoT.Platform.Network.Router.Application
{
	public class Startup
	{
		private readonly IConfiguration Configuration;

		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Database").Bind(db);
			this.Configuration.GetSection("Mqtt").Bind(mqtt);

			var reload = this.Configuration.GetValue<int>("Cache:DataReloadInterval");
			var capacity = this.Configuration.GetValue<int>("Cache:Capacity");
			var timeout = this.Configuration.GetValue<int>("Cache:Timeout");
			var privatemqtt = mqtt.InternalBroker;
			var publicmqtt = mqtt.PublicBroker;

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddAuthorizationContext(db.SensateIoT.ConnectionString);
			services.AddNetworkingContext(db.Networking.ConnectionString);

			services.Configure<DataReloadSettings>(opts => {
				opts.StartDelay = TimeSpan.FromSeconds(1);
				opts.DataReloadInterval = TimeSpan.FromSeconds(reload);
				opts.LiveDataReloadInterval = TimeSpan.FromSeconds(this.Configuration.GetValue<int>("Cache:LiveDataReloadInterval"));
				opts.TimeoutScanInterval = TimeSpan.FromSeconds(this.Configuration.GetValue<int>("Cache:TimeoutScanInterval"));
			});

			services.Configure<DataCacheOptions>(opts => {
				opts.Capacity = capacity;
				opts.Timeout = TimeSpan.FromSeconds(timeout);
			});

			services.Configure<QueueSettings>(s => {
				s.LiveDataQueueTemplate = this.Configuration.GetValue<string>("Routing:LiveDataTopic");
				s.TriggerQueueTemplate = this.Configuration.GetValue<string>("Routing:TriggerTopic");
				s.MessageStorageQueueTopic = this.Configuration.GetValue<string>("Routing:MessageStorageQueueTopic");
				s.MeasurementStorageQueueTopic = this.Configuration.GetValue<string>("Routing:MeasurementStorageQueueTopic");
				s.NetworkEventQueueTopic = this.Configuration.GetValue<string>("Routing:NetworkEventQueueTopic");
			});

			services.Configure<RoutingPublishSettings>(s => {
				s.PublicInterval = TimeSpan.FromMilliseconds(this.Configuration.GetValue<int>("Routing:InternalPublishInterval"));
				s.InternalInterval = TimeSpan.FromMilliseconds(this.Configuration.GetValue<int>("Routing:PublicPublishInterval"));
				s.ActuatorTopicFormat = this.Configuration.GetValue<string>("Routing:ActuatorTopicFormat");
			});

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
			});

			services.AddMqttService(options => {
				options.Ssl = publicmqtt.Ssl;
				options.Host = publicmqtt.Host;
				options.Port = publicmqtt.Port;
				options.Username = publicmqtt.Username;
				options.Password = publicmqtt.Password;
				options.Id = Guid.NewGuid().ToString();
			});

			services.AddScoped<ITriggerRepository, TriggerRepository>();
			services.AddScoped<IRoutingRepository, RoutingRepository>();
			services.AddScoped<ILiveDataHandlerRepository, LiveDataHandlerRepository>();

			services.AddSingleton<IHostedService, SensorReloadService>();
			services.AddSingleton<IHostedService, AccountReloadService>();
			services.AddSingleton<IHostedService, ApiKeyReloadService>();
			services.AddSingleton<IHostedService, LiveDataHandlerReloadService>();
			services.AddSingleton<IHostedService, RoutingPublishService>();
			services.AddSingleton<IHostedService, ActuatorPublishService>();
			services.AddSingleton<IHostedService, LiveDataReloadService>();
			services.AddSingleton<IHostedService, CacheTimeoutScanService>();

			services.AddSingleton<IQueue<IPlatformMessage>, Deque<IPlatformMessage>>();
			services.AddSingleton<IMessageQueue, MessageQueue>();
			services.AddSingleton<IRemoteNetworkEventQueue, RemoteNetworkEventQueue>();
			services.AddSingleton<IInternalRemoteQueue, InternalMqttQueue>();
			services.AddSingleton<IPublicRemoteQueue, PublicMqttQueue>();
			services.AddSingleton<IHostedService, RoutingService>();
			services.AddSingleton<IAuthorizationService, AuthorizationService>();
			services.AddSingleton<IRemoteStorageQueue, RemoteStorageQueue>();
			services.AddSingleton<IDataCache, DataCache>();

			services.AddGrpc();
			services.AddGrpcReflection();
			services.AddMqttHandlers();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();
			provider.MapInternalMqttTopic<CommandConsumer>(mqtt.InternalBroker.CommandTopic);

			app.UseEndpoints(endpoints => {
				endpoints.MapGrpcService<IngressRouter>();
				endpoints.MapGrpcService<EgressRouter>();

				if(env.IsDevelopment()) {
					endpoints.MapGrpcReflectionService();
				}
			});
		}
	}
}
