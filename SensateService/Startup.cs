/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;

using SensateService.Models;
using SensateService.Infrastructure.Sql;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Cache;
using SensateService.Services;
using SensateService.Controllers;

using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

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
			services.AddIdentity<SensateUser, IdentityRole>()
				.AddEntityFrameworkStores<SensateSqlContext>()
				.AddDefaultTokenProviders();
			JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
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
					ValidAudience = Secrets["JwtIsuer"],
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
			services.AddScoped<IUserRepository, UserRepository>();
			if(this.Configuration["Cache"] == "true") {
				if(this.Configuration["CacheType"] == "Distributed")
					services.AddScoped<ICacheStrategy<string>, DistributedCacheStrategy>();
				else
					services.AddScoped<ICacheStrategy<string>, MemoryCacheStrategy>();

				Debug.WriteLine("Caching enabled!");
				services.AddScoped<IMeasurementRepository, CachedMeasurementRepository>();
				services.AddScoped<ISensorRepository, CachedSensorRepository>();
			} else {
				Debug.WriteLine("Caching disabled!");
				services.AddScoped<IMeasurementRepository, StandardMeasurementRepository>();
				services.AddScoped<ISensorRepository, StandardSensorRepository>();
			}

			var mqttopts = new MqttOptions();

			mqttopts.Ssl = Configuration["MqttSsl"] == "true";
			mqttopts.Host = Configuration["MqttHost"];
			mqttopts.Port = Int32.Parse(Configuration["MqttPort"]);
			mqttopts.Username = Secrets["MqttUsername"];
			mqttopts.Password = Secrets["MqttPassword"];
			mqttopts.Id = Guid.NewGuid().ToString();
			mqttopts.Topic = $"sensate/{Configuration["InstanceName"]}/{Program.ApiVersionString}";

			Program.MqttClient = MqttServiceFactory.CreateMqttService(
				services.BuildServiceProvider().CreateScope().ServiceProvider,
				mqttopts
			);

			try {
				var result = Program.MqttClient.ConnectAsync();
				result.Wait();
			} catch(Exception ex) {
				Debug.WriteLine($"Potential MQTT error: {ex.Message}");
			}

			services.AddMvc();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
