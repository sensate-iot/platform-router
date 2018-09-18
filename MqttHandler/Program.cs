/*
 * SensateService.Mqtt program entry.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace SensateService.MqttHandler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceCollection services;

            Console.WriteLine($"Starting Sensate MQTT client Sensate CORE {Version.VersionString}");
            services = new ServiceCollection();
			var starter = new Startup();
            starter.ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            starter.Configure(provider);
            starter.Run(provider);
        }
    }
}
