/*
 * Program startup.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;
using SensateService.Services;

using SensateService.MqttHandler.Mqtt;
using SensateService.Services.Processing;

namespace SensateService.MqttHandler.Application
{
	public class Startup
	{
		private readonly IConfiguration Configuration;

		public Startup()
		{
			var builder = new ConfigurationBuilder();

			builder.SetBasePath(Path.Combine(AppContext.BaseDirectory));
			builder.AddEnvironmentVariables();

			if(IsDevelopment())
				builder.AddUserSecrets<Startup>();
			else
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


			Configuration = builder.Build();
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
			var mqtt = new MqttConfig();
			var db = new DatabaseConfig();
			var cache = new CacheConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			Configuration.GetSection("Database").Bind(db);
			Configuration.GetSection("Cache").Bind(cache);

			services.AddPostgres(db.PgSQL.ConnectionString);

			services.AddIdentity<SensateUser, UserRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });


			if(cache.Enabled)
                services.AddCacheStrategy(cache, db);

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSqlRepositories();

			services.AddMqttService(options => {
				options.Ssl = mqtt.Ssl;
				options.Host = mqtt.Host;
				options.Port = mqtt.Port;
				options.Username = mqtt.Username;
				options.Password = mqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/sensate/";
				options.InternalMeasurementTopic = mqtt.InternalMeasurementTopic;
			});

			services.AddSingleton(provider => {
				var s = provider.GetServices<IHostedService>().ToList();
				var mqservice = s.Find(x => x.GetType() == typeof(MqttService)) as IMqttPublishService;
				return mqservice;
			});

			services.AddLogging((builder) => {
				if(!IsDevelopment())
					return;

				builder.AddConsole();
				builder.AddDebug();
			});
		}

		public void Configure(IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			provider.MapMqttTopic<MqttMeasurementHandler>(mqtt.ShareTopic);
		}

		public Application BuildApplication(IServiceProvider sp)
		{
			return new Application(sp);
		}
	}
}
