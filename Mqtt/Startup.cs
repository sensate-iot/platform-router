/*
 * Program startup.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SensateService.Init;
using Microsoft.Extensions.Hosting;
using SensateService.Infrastructure.Sql;
using SensateService.Models;

namespace SensateService.Mqtt
{
	public class Startup
	{
		public IConfiguration Configuration;
		private ManualResetEvent _reset;

		public Startup()
		{
			var builder = new ConfigurationBuilder();
			string env;

			this._reset = new ManualResetEvent(false);
			Console.CancelKeyPress += this.CancelEvent_Handler;

			builder.SetBasePath(Path.Combine(AppContext.BaseDirectory));
			builder.AddJsonFile("appsettings.json", optional:false);
			builder.AddEnvironmentVariables();

			env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			if(String.IsNullOrEmpty(env) || env == "Development") {
				builder.AddUserSecrets<Startup>();
			}

			Configuration = builder.Build();
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this._reset.Set();
			e.Cancel = true;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			string pgsql;

			pgsql = this.Configuration["PgSqlConnectionString"];
			services.AddPostgres(pgsql);

			services.AddIdentity<SensateUser, UserRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			services.AddLogging();
			services.AddDocumentStore(Configuration["MongoDbConnectionString"], Configuration["MongoDbDatabaseName"]);

			services.AddDistributedRedisCache(opts => {
				opts.Configuration = Configuration["RedisHost"];
				opts.InstanceName = Configuration["RedisInstanceName"];
			});

			services.AddCacheStrategy(Configuration["CacheType"]);
			services.AddDocumentRepositories(Configuration["Cache"] == "true");
			services.AddSqlRepositories();

			services.AddMqttService(options => {
				options.Ssl = Configuration["MqttSsl"] == "true";
				options.Host = Configuration["MqttHost"];
				options.Port = Int32.Parse(Configuration["MqttPort"]);
				options.Username = Configuration["MqttUsername"];
				options.Password = Configuration["MqttPassword"];
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/sensate/";
			});
		}

		public void Configure(IServiceProvider provider)
		{
			provider.MapMqttTopic<MqttMeasurementHandler>(Configuration["MqttShareTopic"]);
		}

		public void Run(IServiceProvider provider)
		{
			var services = provider.GetServices<IHostedService>().ToList();

			services.ForEach(x => x.StartAsync(CancellationToken.None));
			Console.WriteLine("MQTT client started");
			this._reset.WaitOne();
		}
	}
}
