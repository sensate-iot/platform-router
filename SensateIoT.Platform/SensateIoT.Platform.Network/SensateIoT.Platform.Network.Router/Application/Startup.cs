/*
 * Router service startup.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Common.Services.Data;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.Router.Config;

namespace SensateIoT.Platform.Network.Router.Application
{
	public class Startup
	{
		private readonly IConfiguration Configuration;

		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();

			this.Configuration.GetSection("Database").Bind(db);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddAuthorizationContext(db.SensateIoT.ConnectionString);
			services.AddTriggerContext(db.SensateIoT.ConnectionString);

			services.Configure<DataReloadSettings>(opts => {
				opts.StartDelay = TimeSpan.Zero;
				opts.ReloadInterval = TimeSpan.FromMinutes(5);
			});

			services.Configure<DataCacheSettings>(opts => {
				opts.Capacity = 25000;
				opts.Timeout = TimeSpan.FromMinutes(6);
			});

			services.AddScoped<ITriggerRepository, TriggerRepository>();
			services.AddScoped<IAccountsRepository, AccountsRepository>();
			services.AddScoped<ISensorRepository, SensorRepository>();
			services.AddSingleton<IHostedService, SensorReloadService>();
			services.AddSingleton<IDataCache, DataCache>();
			services.AddGrpc();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints => {

			});
		}
	}
}
