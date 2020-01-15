/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using SensateService.ApiCore.Init;
using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;

namespace SensateService.WebSocketHandler.Application
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
			var auth = new AuthenticationConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);
			Configuration.GetSection("Database").Bind(db);
			Configuration.GetSection("Authentication").Bind(auth);
			Configuration.GetSection("Cache").Bind(cache);

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });

			if(cache.Enabled)
				services.AddCacheStrategy(cache, db);

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSqlRepositories(cache.Enabled);
			services.AddMeasurementStorage(cache);

			this.SetupAuthentication(services, auth);

			services.AddInternalMqttService(options => {
				options.Ssl = mqtt.InternalBroker.Ssl;
				options.Host = mqtt.InternalBroker.Host;
				options.Port = mqtt.InternalBroker.Port;
				options.Username = mqtt.InternalBroker.Username;
				options.Password = mqtt.InternalBroker.Password;
				options.Id = Guid.NewGuid().ToString();
				options.InternalBulkMeasurementTopic = mqtt.InternalBroker.InternalBulkMeasurementTopic;
				options.InternalMeasurementTopic = mqtt.InternalBroker.InternalMeasurementTopic;
			});

			services.AddSingleton<IHostedService, MqttPublishHandler>();

			services.AddWebSocketHandler<RealTimeWebSocketMeasurementHandler>();
			services.AddWebSocketHandler<WebSocketBulkMeasurementHandler>();
			services.AddWebSocketHandler<WebSocketMeasurementHandler>();
			services.AddControllers().AddNewtonsoftJson();

			services.AddLogging((builder) => {
				builder.AddConsole();

				if(IsDevelopment())
					builder.AddDebug();
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logging, IServiceProvider sp)
		{
			var cache = new CacheConfig();

			app.UseRouting();
			app.UseWebSockets();
			Configuration.GetSection("Cache").Bind(cache);

			app.MapWebSocketService("/measurement/rt", sp.GetService<RealTimeWebSocketMeasurementHandler>());
			app.MapWebSocketService("/measurement", sp.GetService<WebSocketMeasurementHandler>());
			app.MapWebSocketService("/measurement/bulk", sp.GetService<WebSocketBulkMeasurementHandler>());
			app.UseAuthorization();
			app.UseAuthorization();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}

		private void SetupAuthentication(IServiceCollection services, AuthenticationConfig auth)
		{
			services.AddIdentity<SensateUser, SensateRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			services.Configure<UserAccountSettings>(options => {
				options.JwtKey = auth.JwtKey;
				options.JwtIssuer = auth.JwtIssuer;
				options.JwtExpireMinutes = auth.JwtExpireMinutes;
				options.JwtRefreshExpireMinutes = auth.JwtRefreshExpireMinutes;
				options.PublicUrl = auth.PublicUrl;
				options.Scheme = auth.Scheme;
			});

			services.Configure<IdentityOptions>(options => {
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 8;
				options.Password.RequiredUniqueChars = 5;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				options.User.RequireUniqueEmail = true;
			});

			services.AddAuthentication(options => {
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(cfg => {
				cfg.RequireHttpsMetadata = false;
				cfg.SaveToken = true;
				cfg.TokenValidationParameters = new TokenValidationParameters
				{
					ValidIssuer = auth.JwtIssuer,
					ValidAudience = auth.JwtIssuer,
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(auth.JwtKey)
					),
					ClockSkew = TimeSpan.Zero
				};
			});
		}
	}
}
