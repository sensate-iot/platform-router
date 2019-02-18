/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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
using SensateService.Services;
using SensateService.WebSocketHandler.Handlers;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
			services.AddSqlRepositories();

			this.SetupAuthentication(services, auth);

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

			services.AddWebSocketHandler<WebSocketMeasurementHandler>();
			services.AddWebSocketHandler<WebSocketLiveMeasurementHandler>();

			services.AddLogging((builder) => {
				builder.AddConsole();

				if(IsDevelopment())
					builder.AddDebug();
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logging, IServiceProvider sp)
		{
			var mqtt = new MqttConfig();

			Configuration.GetSection("Mqtt").Bind(mqtt);

			app.UseWebSockets();
			app.MapWebSocketService("/measurement", sp.GetService<WebSocketMeasurementHandler>());
			app.MapWebSocketService("/measurements/live", sp.GetService<WebSocketLiveMeasurementHandler>());
			sp.MapMqttTopic<MqttInternalMeasurementHandler>(mqtt.InternalMeasurementTopic);
		}

		private void SetupAuthentication(IServiceCollection services, AuthenticationConfig auth)
		{
			services.AddIdentity<SensateUser, UserRole>(config => {
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
