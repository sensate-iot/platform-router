/*
 * API application startup.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.Adapters.Blobs;
using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Authorization;
using SensateIoT.Platform.Network.API.Config;
using SensateIoT.Platform.Network.API.Middleware;
using SensateIoT.Platform.Network.API.MQTT;
using SensateIoT.Platform.Network.API.Services;
using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;

namespace SensateIoT.Platform.Network.API.Application
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Database").Bind(db);
			this.Configuration.GetSection("Mqtt").Bind(mqtt);

			var privatemqtt = mqtt.InternalBroker;

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddAuthorizationContext(db.SensateIoT.ConnectionString);
			services.AddNetworkingContext(db.Networking.ConnectionString);

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
			});

			services.Configure<InternalBrokerConfig>(this.Configuration.GetSection("Mqtt:InternalBroker"));
			services.Configure<RouterConfig>(this.Configuration.GetSection("Router"));
			services.Configure<BlobOptions>(this.Configuration.GetSection("Storage"));

			services.AddScoped<ITriggerRepository, TriggerRepository>();
			services.AddScoped<IMessageRepository, MessageRepository>();
			services.AddScoped<IControlMessageRepository, ControlMessageRepository>();
			services.AddScoped<IMeasurementRepository, MeasurementRepository>();
			services.AddScoped<ISensorRepository, SensorRepository>();
			services.AddScoped<ISensorLinkRepository, SensorLinkRepository>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
			services.AddScoped<ILiveDataHandlerRepository, LiveDataHandlerRepository>();
			services.AddScoped<ISensorService, SensorService>();
			services.AddScoped<ICommandPublisher, CommandPublisher>();
			services.AddScoped<IBlobRepository, BlobRepository>();

			services.AddSingleton<IBlobService, FilesystemBlobService>();
			services.AddSingleton<IRouterClient, RouterClient>();
			services.AddSingleton<IMeasurementAuthorizationService, MeasurementAuthorizationService>();
			services.AddSingleton<IHashAlgorithm, SHA256Algorithm>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddHostedService<BatchRoutingService>();

			services.AddRouting();
			services.AddControllers().AddNewtonsoftJson();
			services.AddMqttHandlers();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);

			app.UseForwardedHeaders();
			app.UseRouting();

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			app.UseMiddleware<ApiKeyValidationMiddleware>();
			provider.MapInternalMqttTopic<CommandConsumer>(mqtt.InternalBroker.CommandTopic);

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
