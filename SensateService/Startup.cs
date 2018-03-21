/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;

using SensateService.Models;
using SensateService.Infrastructure.Sql;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Cache;
using SensateService.Init;
using SensateService.Services;
using SensateService.Controllers;
using SensateService.Middleware;
using Microsoft.Extensions.Hosting;

namespace SensateService
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment environment)
		{
			var builder = new ConfigurationBuilder()
							.SetBasePath(environment.ContentRootPath)
							.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
							.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

			if(environment.IsDevelopment()) {
				builder.AddUserSecrets<Startup>();
				builder.AddApplicationInsightsSettings(developerMode: true);
			}

			builder.AddEnvironmentVariables();
			this.Secrets = builder.Build();
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public IConfiguration Secrets {get;}

		public void ConfigureServices(IServiceCollection services)
		{
			string pgsql, mongo;

			pgsql = this.Secrets.GetValue<string>("PgSqlConnectionString");
			mongo = this.Secrets.GetValue<string>("MongoDbConnectionString");
			services.AddEntityFrameworkNpgsql()
					.AddDbContext<SensateSqlContext>(options => {
				options.UseNpgsql(pgsql);
			});

			services.Configure<MongoDBSettings>(options => {
				options.ConnectionString = Secrets["MongoDbConnectionString"];
				options.DatabaseName = Secrets["MongoDbDatabaseName"];
			});

			services.Configure<UserAccountSettings>(options => {
				options.JwtKey = this.Secrets["JwtKey"];
				options.JwtIssuer = this.Secrets["JwtIssuer"];
				options.JwtExpireDays = Int32.Parse(this.Secrets["JwtExpireDays"]);
			});

			services.AddTransient<SensateContext>();
			
			services.AddApiVersioning(options => {
				options.ApiVersionReader = new QueryStringApiVersionReader();
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
			});

			/*
			 * Setup user authentication
			 */
			services.AddIdentity<SensateUser, SensateRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

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
					ValidIssuer = Secrets["JwtIssuer"],
					ValidAudience = Secrets["JwtIssuer"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(Secrets["JwtKey"])
					),
					ClockSkew = TimeSpan.Zero
				};
			});

			services.AddDistributedRedisCache(opts => {
				opts.Configuration = Configuration["RedisHost"];
				opts.InstanceName = Configuration["RedisInstanceName"];
			});

			/* Add repositories */
			services.AddSqlRepositories();
			services.AddCacheStrategy(Configuration["CacheType"]);
			services.AddMongoDbRepositories(Configuration["Cache"] == "true");

			services.AddMqttService(options => {
				options.Ssl = Configuration["MqttSsl"] == "true";
				options.Host = Configuration["MqttHost"];
				options.Port = Int32.Parse(Configuration["MqttPort"]);
				options.Username = Secrets["MqttUsername"];
				options.Password = Secrets["MqttPassword"];
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/sensate/";
			});

			services.AddSingleton<IEmailSender, EmailSender>();
			services.Configure<MessageSenderAuthOptions>(opts => {
				opts.FromName = Configuration["EmailFromName"];
				opts.From = Configuration["EmailFrom"];
				opts.Key = Secrets["SendGridKey"];
				opts.Username = Secrets["SendGridUser"];
			});

			services.AddWebSocketService();
			services.AddMvc();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider sp)
		{
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.MapMqttTopic<MqttMeasurementHandler>(sp, Configuration["MqttShareTopic"]);
			app.UseWebSockets();
			app.MapWebSocketService("/measurement", sp.GetService<WebSocketMeasurementHandler>());
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
