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
using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Common.Services.Data;
using SensateIoT.Platform.Network.Common.Services.Processing;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.Router.Config;
using SensateService.Init;

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
			services.AddTriggerContext(db.SensateIoT.ConnectionString);

			services.Configure<DataReloadSettings>(opts => {
				opts.StartDelay = TimeSpan.FromSeconds(1);
				opts.ReloadInterval = TimeSpan.FromMinutes(timeout);
			});

			services.Configure<DataCacheSettings>(opts => {
				opts.Capacity = capacity;
				opts.Timeout = TimeSpan.FromMinutes(reload);
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
			services.AddScoped<IAccountsRepository, AccountsRepository>();
			services.AddScoped<ISensorRepository, SensorRepository>();

			services.AddSingleton<IHostedService, SensorReloadService>();
			services.AddSingleton<IHostedService, AccountReloadService>();
			services.AddSingleton<IHostedService, ApiKeyReloadService>();

			//services.AddSingleton<IQueue<IPlatformMessage>, Deque<IPlatformMessage>>();
			services.AddSingleton<IRoutingService, RoutingService>();
			services.AddSingleton<IHostedService>(p => p.GetRequiredService<IRoutingService>());

			services.AddSingleton<IDataCache, DataCache>();
			services.AddGrpc();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints => {

			});
		}
	}
}
