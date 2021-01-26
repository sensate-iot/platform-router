/*
 * API application startup.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.Adapters.Blobs;
using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Authorization;
using SensateIoT.Platform.Network.API.Config;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.API.Extensions;
using SensateIoT.Platform.Network.API.Middleware;
using SensateIoT.Platform.Network.API.MQTT;
using SensateIoT.Platform.Network.API.Services;
using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Common.Services.Metrics;
using SensateIoT.Platform.Network.Common.Services.Processing;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;

using IAuthorizationService = SensateIoT.Platform.Network.Common.Services.Processing.IAuthorizationService;

namespace SensateIoT.Platform.Network.API.Application
{
	public class Startup
	{
		private readonly IConfiguration m_configuration;

		public Startup(IConfiguration configuration)
		{
			this.m_configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();
			var mqtt = new MqttConfig();
			var cache = new CacheConfig();

			this.m_configuration.GetSection("Database").Bind(db);
			this.m_configuration.GetSection("Mqtt").Bind(mqtt);
			this.m_configuration.GetSection("Cache").Bind(cache);

			var proxyLevel = this.m_configuration.GetValue<int>("System:ProxyLevel");
			var privatemqtt = mqtt.InternalBroker;

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddAuthorizationContext(db.SensateIoT.ConnectionString);
			services.AddNetworkingContext(db.Networking.ConnectionString);
			services.AddDistributedCaches<PaginationResponse<Sensor>>(cache.Host, cache.Port);
			services.AddCors();

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
			});

			services.Configure<InternalBrokerConfig>(this.m_configuration.GetSection("Mqtt:InternalBroker"));
			services.Configure<RouterConfig>(this.m_configuration.GetSection("Router"));
			services.Configure<BlobOptions>(this.m_configuration.GetSection("Storage"));
			services.Configure<MetricsOptions>(this.m_configuration.GetSection("HttpServer:Metrics"));

			services.AddScoped<ITriggerRepository, TriggerRepository>();
			services.AddScoped<IMessageRepository, MessageRepository>();
			services.AddScoped<IControlMessageRepository, ControlMessageRepository>();
			services.AddScoped<IMeasurementRepository, MeasurementRepository>();
			services.AddScoped<ISensorRepository, SensorRepository>();
			services.AddScoped<ISensorLinkRepository, SensorLinkRepository>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
			services.AddScoped<ILiveDataHandlerRepository, LiveDataHandlerRepository>();
			services.AddScoped<ISensorService, SensorService>();
			services.AddScoped<ICommandPublisher, CommandPublisher>();
			services.AddScoped<IBlobRepository, BlobRepository>();
			services.AddScoped<IAuditLogRepository, AuditLogRepository>();

			services.AddSingleton<IBlobService, FilesystemBlobService>();
			services.AddSingleton<IRouterClient, RouterClient>();
			services.AddSingleton<IAuthorizationService, AuthorizationService>();
			services.AddSingleton<IMeasurementAuthorizationService, MeasurementAuthorizationService>();
			services.AddSingleton<IMessageAuthorizationService, MessageAuthorizationService>();
			services.AddSingleton<IHashAlgorithm, SHA256Algorithm>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddHostedService<BatchRoutingService>();
			services.AddHostedService<MetricsService>();

			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo {
					Title = "Sensate IoT Network API - Version 1",
					Version = "v1"
				});

				c.SchemaFilter<ObjectIdSchemaFilter>();
				c.OperationFilter<ObjectIdOperationFilter>();

				c.AddSecurityDefinition("X-ApiKey", new OpenApiSecurityScheme {
					In = ParameterLocation.Header,
					Name = "X-ApiKey",
					Type = SecuritySchemeType.ApiKey,
					Description = "API key needed to access the endpoints."
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Name = "X-ApiKey",
							Type = SecuritySchemeType.ApiKey,
							In = ParameterLocation.Header,
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "X-ApiKey"
							},
						},
						Array.Empty<string>()
					}
				});
			});

			services.AddReverseProxy(proxyLevel);
			services.AddRouting();
			services.AddControllers().AddNewtonsoftJson();
			services.AddMqttHandlers();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.m_configuration.GetSection("Mqtt").Bind(mqtt);

			app.UseForwardedHeaders();
			app.UseRouting();

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			app.UseSwagger(c => {
				c.RouteTemplate = "network/swagger/{documentName}/swagger.json";
			});

			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/network/swagger/v1/swagger.json", "Sensate Network API v1");
				c.RoutePrefix = "network/swagger";
			});

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			app.UseMiddleware<ErrorLoggingMiddleware>();
			app.UseMiddleware<ApiKeyValidationMiddleware>();
			app.UseMiddleware<RequestLoggingMiddleware>();
			app.UseMiddleware<JsonErrorHandlerMiddleware>();

			provider.MapInternalMqttTopic<CommandConsumer>(mqtt.InternalBroker.CommandTopic);

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
