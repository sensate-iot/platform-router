using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Router.Common.Settings;

using Serilog;

namespace SensateIoT.Platform.Router.Service.Init
{
	public static class KestrelEndpointConfigurationExtensions
	{
		public static void ConfigureEndpoints(this KestrelServerOptions options)
		{
			var configuration = options.ApplicationServices.GetRequiredService<IConfiguration>();

			var endpoints = configuration.GetSection("HttpServer:Endpoints")
				.GetChildren()
				.ToDictionary(section => section.Key, section => {
					var endpoint = new EndpointConfigurationSettings();
					section.Bind(endpoint);
					return endpoint;
				});

			foreach(var endpoint in endpoints) {
				CreateEndPoint(options, endpoint.Value);
			}
		}

		private static void CreateEndPoint(KestrelServerOptions options, EndpointConfigurationSettings endpoint)
		{
			var environment = options.ApplicationServices.GetRequiredService<IHostEnvironment>();
			var port = endpoint.Port ?? (endpoint.Scheme == "https" ? 443 : 80);
			var ipAddresses = new List<IPAddress>();

			if(endpoint.Host == "localhost") {
				Log.Logger.Information("Binding to localhost:{port}.", port);
				ipAddresses.Add(IPAddress.Loopback);
			} else if(IPAddress.TryParse(endpoint.Host, out var address)) {
				Log.Logger.Information("Binding to {ipAddress}:{port}.", address.ToString(), port);
				ipAddresses.Add(address);
			} else {
				Log.Logger.Information("Binding to IPAddress.Any:{port}", port);
				ipAddresses.Add(IPAddress.Any);
			}

			foreach(var address in ipAddresses) {
				options.Listen(address, port,
					listenOptions => {
						listenOptions.Protocols = GetHttpVersion(endpoint);

						if(endpoint.Scheme != "https") {
							return;
						}

						var certificate = LoadCertificate(endpoint, environment);
						listenOptions.UseHttps(certificate);
					});
			}
		}

		private static HttpProtocols GetHttpVersion(EndpointConfigurationSettings options)
		{
			return options.Version switch {
				2 => HttpProtocols.Http2,
				_ => HttpProtocols.Http1
			};
		}

		private static X509Certificate2 LoadCertificate(EndpointConfigurationSettings config, IHostEnvironment environment)
		{
			if(config.StoreName != null && config.StoreLocation != null) {
				using var store = new X509Store(config.StoreName, Enum.Parse<StoreLocation>(config.StoreLocation));
				store.Open(OpenFlags.ReadOnly);
				var certificate = store.Certificates.Find(
					X509FindType.FindBySubjectName,
					config.Host,
					environment.IsDevelopment()
				);

				if(certificate.Count == 0) {
					throw new InvalidOperationException($"Certificate not found for {config.Host}.");
				}

				return certificate[0];
			}

			if(config.FilePath != null && config.Password != null) {
				return new X509Certificate2(config.FilePath, config.Password);
			}

			throw new InvalidOperationException("No valid certificate configuration found for the current endpoint.");
		}
	}
}
