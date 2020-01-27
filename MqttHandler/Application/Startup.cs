/*
 * Program startup.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.IO;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;
using SensateService.MqttHandler.Mqtt;

namespace SensateService.MqttHandler.Application
{
	public class Startup
	{
		private readonly IConfiguration Configuration;

		/*public Startup()
		{
			var builder = new ConfigurationBuilder();

			builder.SetBasePath(Path.Combine(AppContext.BaseDirectory));
			builder.AddEnvironmentVariables();

			if(IsDevelopment())
				builder.AddUserSecrets<Startup>();
			else
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


			Configuration = builder.Build();
		}*/

		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		private static bool IsDevelopment()
		{
#if DEBUG
			var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			return env == "Development";
#else
			return false;
#endif

		}

		public void ConfigureServices(IServiceCollection services)
		{
			var mqtt = new MqttConfig();
			var db = new DatabaseConfig();
			var cache = new CacheConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			Configuration.GetSection("Database").Bind(db);
			Configuration.GetSection("Cache").Bind(cache);

			var publicmqtt = mqtt.PublicBroker;
			var privatemqtt = mqtt.InternalBroker;

			services.AddPostgres(db.PgSQL.ConnectionString);

			services.AddIdentity<SensateUser, SensateRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });

			if(cache.Enabled)
                services.AddCacheStrategy(cache, db);

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSqlRepositories(cache.Enabled);
			services.AddMeasurementStorage(cache);

			services.AddMqttService(options => {
				options.Ssl = publicmqtt.Ssl;
				options.Host = publicmqtt.Host;
				options.Port = publicmqtt.Port;
				options.Username = publicmqtt.Username;
				options.Password = publicmqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/sensate/";
			});

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.InternalBulkMeasurementTopic = privatemqtt.InternalBulkMeasurementTopic;
				options.InternalMeasurementTopic = privatemqtt.InternalMeasurementTopic;
			});

			services.AddSingleton<IHostedService, MqttPublishHandler>();

			services.AddLogging(builder => {
				builder.AddConfiguration(Configuration.GetSection("Logging"));
				if(IsDevelopment())
					builder.AddDebug();

				builder.AddConsole();
			});
		}

		public void Configure(IServiceProvider provider)
		{
			var mqtt = new MqttConfig();
			var cache = new CacheConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			Configuration.GetSection("Cache").Bind(cache);
			var @public = mqtt.PublicBroker;

			provider.MapMqttTopic<MqttRealTimeMeasurementHandler>(@public.RealTimeShareTopic);
			provider.MapMqttTopic<MqttMeasurementHandler>(@public.ShareTopic);
			provider.MapMqttTopic<MqttBulkMeasurementHandler>(@public.BulkShareTopic);
		}
	}
}

