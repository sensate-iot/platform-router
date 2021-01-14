/*
 * Dashboard API startup.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using SensateIoT.API.Common.ApiCore.Init;
using SensateIoT.API.Common.ApiCore.Middleware;
using SensateIoT.API.Common.Config.Config;
using SensateIoT.API.Common.Core.Init;

namespace SensateIoT.API.DashboardApi.Application
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		// ReSharper disable once UnusedMember.Global
		public void ConfigureServices(IServiceCollection services)
		{
			var cache = new CacheConfig();
			var db = new DatabaseConfig();
			var auth = new AuthenticationConfig();
			var sys = new SystemConfig();

			this._configuration.GetSection("System").Bind(sys);
			this._configuration.GetSection("Cache").Bind(cache);
			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Database").Bind(db);

			services.AddCors();

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddIdentityFramwork(auth);
			services.AddReverseProxy(sys);

			if(cache.Enabled) {
				services.AddCacheStrategy(cache, db);
			}

			/* Add repositories */
			services.AddSqlRepositories(cache.Enabled);
			services.AddDocumentRepositories(cache.Enabled);

			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo {
					Title = "Sensate IoT Dashboard API - Version 1",
					Version = "v1"
				});

				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
					In = ParameterLocation.Header,
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Description = "API key needed to access the endpoints."
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Name = "Authorization",
							Type = SecuritySchemeType.ApiKey,
							In = ParameterLocation.Header,
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
						},
						new string[] {}
					}
				});
			});


			services.AddRouting();
			services.AddControllers().AddNewtonsoftJson();
		}

		// ReSharper disable once UnusedMember.Global
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp)
		{
			var auth = new AuthenticationConfig();
			this._configuration.GetSection("Authentication").Bind(auth);

			app.UseForwardedHeaders();
			app.UseRouting();

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			app.UseSwagger(c => {
				c.RouteTemplate = "stats/swagger/{documentName}/swagger.json";
			});

			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/stats/swagger/v1/swagger.json", "Sensate Dashboard API v1");
				c.RoutePrefix = "stats/swagger";
			});

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseMiddleware<RequestLoggingMiddleware>();
			app.UseMiddleware<UserFetchMiddleware>();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}
	}
}
