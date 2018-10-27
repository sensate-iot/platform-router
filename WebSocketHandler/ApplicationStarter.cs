/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Init;
using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;
using SensateService.Services;

using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace SensateService.WebSocketHandler
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
			var mqtt = new MqttConfig();
			var db = new DatabaseConfig();
			var cache = new CacheConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			Configuration.GetSection("Database").Bind(db);
			Configuration.GetSection("Cache").Bind(cache);

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });

			if(cache.Enabled)
				services.AddCacheStrategy(cache, db);

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSqlRepositories();

			services.AddIdentity<SensateUser, UserRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

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

			services.AddWebSocketService();
			services.AddWebSocketHandler<WebSocketMeasurementHandler>();
			services.AddWebSocketHandler<WebSocketLiveMeasurementHandler>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logging, IServiceProvider sp)
		{
			var mqtt = new MqttConfig();

			logging.AddConsole();
			Configuration.GetSection("Mqtt").Bind(mqtt);

			if(IsDevelopment())
				logging.AddDebug();

			app.UseWebSockets();
			app.MapWebSocketService("/measurement", sp.GetService<WebSocketMeasurementHandler>());
			sp.MapMqttTopic<MqttInternalMeasurementHandler>(mqtt.InternalMeasurementTopic);
		}
	}
}
