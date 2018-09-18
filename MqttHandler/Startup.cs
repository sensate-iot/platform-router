/*
 * Program startup.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensateService.Config;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;

namespace SensateService.MqttHandler
{
	public class Startup
	{
		private readonly IConfiguration Configuration;
		private readonly ManualResetEvent _reset;

		public Startup()
		{
			var builder = new ConfigurationBuilder();

			this._reset = new ManualResetEvent(false);
			Console.CancelKeyPress += this.CancelEvent_Handler;

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
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			return env == "Development";
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this._reset.Set();
			e.Cancel = true;
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

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName);
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
			});
		}

		public void Configure(IServiceProvider provider)
		{
			var mqtt = new MqttConfig();
			var logging = provider.GetRequiredService<ILoggerFactory>();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			provider.MapMqttTopic<MqttMeasurementHandler>(mqtt.ShareTopic);

			logging.AddConsole();
			if(IsDevelopment())
                logging.AddDebug();
		}

		public void Run(IServiceProvider provider)
		{
			var services = provider.GetServices<IHostedService>().ToList();

			services.ForEach(x => x.StartAsync(CancellationToken.None));
			MeasurementEvents.MeasurementReceived += this.MeasurementReceived;
			Console.WriteLine("MQTT client started");
			this._reset.WaitOne();
		}

		public async Task MeasurementReceived(object sender, MeasurementReceivedEventArgs e)
		{
			Console.WriteLine("Measurement received!");
			await Task.CompletedTask;
		}
	}
}
