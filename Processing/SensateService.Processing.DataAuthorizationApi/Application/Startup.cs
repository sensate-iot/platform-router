/*
 * .NET services start up.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Init;
using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Processing.DataAuthorizationApi.EventHandlers;
using SensateService.Processing.DataAuthorizationApi.Middleware;
using SensateService.Settings;

namespace SensateService.Processing.DataAuthorizationApi.Application
{
	public class Startup
	{
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration, IWebHostEnvironment environment)
		{
			this._configuration = configuration;
			this._env = environment;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var cache = new CacheConfig();
			var db = new DatabaseConfig();
			var auth = new AuthenticationConfig();
			var sys = new SystemConfig();
			var mqtt = new MqttConfig();

			this._configuration.GetSection("System").Bind(sys);
			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Cache").Bind(cache);
			this._configuration.GetSection("Database").Bind(db);
			this._configuration.GetSection("Mqtt").Bind(mqtt);

			var privatemqtt = mqtt.InternalBroker;

			services.AddCors();
			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddIdentityFramwork(auth);
			services.AddSingleton<IHostedService, MqttPublishHandler>();

			if(cache.Enabled) {
				services.AddCacheStrategy(cache, db);
			}

			services.AddAuthorizationServices();

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.InternalBulkMeasurementTopic = privatemqtt.InternalBulkMeasurementTopic;
				options.AuthorizedBulkMeasurementTopic = privatemqtt.AuthorizedBulkMeasurementTopic;
				options.AuthorizedBulkMessageTopic = privatemqtt.AuthorizedBulkMessageTopic;
				options.InternalBulkMessageTopic = privatemqtt.InternalBulkMessageTopic;
			});

			services.Configure<DataAuthorizationSettings>(options => {
				options.MeasurementTopic = privatemqtt.InternalBulkMeasurementTopic;
				options.MessageTopic = privatemqtt.InternalBulkMessageTopic;
			});

			/* Add repositories */
			services.AddSqlRepositories(cache.Enabled);
			services.AddDocumentRepositories(cache.Enabled);

			services.AddRouting();
			services.AddControllers().AddNewtonsoftJson();

			services.AddLogging((logging) => {
				logging.AddConsole();

				if(this._env.IsDevelopment()) {
					logging.AddDebug();
				}
			});

			services.AddMqttHandlers();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp)
		{
			var mqtt = new MqttConfig();

			app.UseForwardedHeaders();
			app.UseRouting();

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			using(var scope = sp.CreateScope()) {
				var ctx = scope.ServiceProvider.GetRequiredService<SensateSqlContext>();
				ctx.Database.EnsureCreated();
				ctx.Database.Migrate();
			}

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			this._configuration.GetSection("Mqtt").Bind(mqtt);

			sp.MapInternalMqttTopic<CommandSubscription>(mqtt.InternalBroker.InternalCommandsTopic);
			app.UseMiddleware<SlimRequestLoggingMiddleware>();
			app.UseMiddleware<ExecutionTimeMeasurementMiddleware>();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}

	}
}
