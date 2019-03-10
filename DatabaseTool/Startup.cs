/*
 * DatabaseTool startup.
 *
 * @author Michel Megens
 * @email dev@bietje.net
 */

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;

namespace SensateService.DatabaseTool
{
	public class Startup
	{
		private readonly IConfiguration Configuration;
		private readonly ManualResetEvent _reset;

		public Startup()
		{
			var builder = new ConfigurationBuilder();

			this._reset = new ManualResetEvent(false);
			Console.CancelKeyPress += this.CancelEvent_Handler;

			builder.SetBasePath(Path.Combine(AppContext.BaseDirectory));
			builder.AddEnvironmentVariables();

			if(IsDevelopment())
				builder.AddUserSecrets<Startup>();
			else
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


			Configuration = builder.Build();
		}

		private static bool IsDevelopment()
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			return env == "Development";
		}

		public void Reset()
		{
			this._reset.Set();
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this._reset.Set();
			e.Cancel = true;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();
			var cache = new CacheConfig();

			Configuration.GetSection("Database").Bind(db);
			Configuration.GetSection("Cache").Bind(cache);

			services.AddPostgres(db.PgSQL.ConnectionString);

			services.AddIdentity<SensateUser, SensateRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });


			if(cache.Enabled)
                services.AddCacheStrategy(cache, db);

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSqlRepositories(cache.Enabled);

			services.AddLogging((builder) => {
				builder.AddConsole();

				if(IsDevelopment())
					builder.AddDebug();
			});
		}

		public void Configure(IServiceProvider provider)
		{
		}

		public async Task Run(IServiceProvider provider)
		{
			CliParser parser = new CliParser();

			var tsk = parser.Run(provider);
			this._reset.WaitOne();
			await tsk;
		}
	}
}