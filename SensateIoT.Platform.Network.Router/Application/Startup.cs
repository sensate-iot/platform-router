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

using Prometheus;
using JetBrains.Annotations;

using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Router.Config;
using SensateIoT.Platform.Network.Router.Init;
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

		[UsedImplicitly]
		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();

			this.Configuration.GetSection("Database").Bind(db);

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddConnectionStrings(db.Networking.ConnectionString, db.SensateIoT.ConnectionString);
			services.AddAuthorizationContext();
			services.AddNetworkingContext();
			services.AddRoutingServices(this.Configuration);
			services.AddBackgroundServices(this.Configuration);
			services.AddMqttBrokers(this.Configuration);
			services.AddMessageRouter();

			services.AddSingleton<CommandCounter>();

			services.AddGrpc();
			services.AddGrpcReflection();
			services.AddMqttHandlers();
		}

		[UsedImplicitly]
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();
			app.UseGrpcMetrics();
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
