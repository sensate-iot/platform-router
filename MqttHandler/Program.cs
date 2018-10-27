/*
 * SensateService.Mqtt program entry.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

using Microsoft.Extensions.DependencyInjection;
using SensateService.MqttHandler.Application;

namespace SensateService.MqttHandler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceCollection services;

            Console.WriteLine($"Starting Sensate MQTT client using {Version.VersionString}");
            services = new ServiceCollection();
			var starter = new Startup();
            starter.ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            starter.Configure(provider);
			starter.BuildApplication(provider).Run();
        }
    }
}
