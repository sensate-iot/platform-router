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
using SensateService.Models.Repositories;
using SensateService.Models.Database;
using SensateService.Middleware;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace sensate_service
{
	public class Startup
	{
		private MqttService _service;

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
			bool result;

			pgsql = this.Secrets.GetValue<string>("PgSqlConnectionString");
			mongo = this.Secrets.GetValue<string>("MongoDbConnectionString");
			services.AddEntityFrameworkNpgsql().AddDbContext<SensateService.Models.Database.SensateSqlContext>(options => {
				options.UseNpgsql(pgsql);
			});

			services.Configure<MongoDBSettings>(options => {
				options.ConnectionString = Secrets["MongoDbConnectionString"];
				options.DatabaseName = Secrets["MongoDbDatabaseName"];
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
				.AddEntityFrameworkStores<SensateService.Models.Database.SensateSqlContext>()
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
			if(this.Configuration["Cache"] == "true") {
				Debug.WriteLine("Caching enabled!");
				services.AddScoped<IMeasurementRepository, CachedMeasurementRepository>();
			} else {
				Debug.WriteLine("Caching disabled!");
				services.AddScoped<IMeasurementRepository, StandardMeasurementRepository>();
			}

			services.AddMvc();

			var scope = services.BuildServiceProvider().CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
			this.testRepo(repo);

			this._service = new MqttService();
			this._service.ConnectAsync().ContinueWith(t => {
				result = t.Result;
			}).Wait();
		}

		private void testRepo(IMeasurementRepository repo)
		{
			Sensor sensor;
			CachedMeasurementRepository cached;

			sensor = new Sensor {
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				InternalId = ObjectId.GenerateNewId(),
				Secret = "yolo",
				Name = "Test sensor",
				Unit = "W"
			};

			dynamic m = new JObject();
			m.Data = 12.11255;
			m.Longitude = 54.2111;
			m.Latitude = 1.232411;
			m.CreatedBySecret = "yolo";

			cached = repo as CachedMeasurementRepository;
			cached.ReceiveMeasurement(sensor, m.ToString());

			//var data = repo.TryGetBetween(sensor, DateTime.Now.AddHours(-5), DateTime.Now.AddHours(-3));
			var data = repo.GetMeasurementsBySensor(sensor);
			var json = data.ToJson();
			Debug.WriteLine(json);
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
