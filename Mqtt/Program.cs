/*
 * SensateService.Mqtt program entry.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using SensateService.Init;

namespace SensateService.Mqtt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceCollection services;

            Console.WriteLine($"Starting SensateService.Mqtt using {Version.VersionString}");
            services = new ServiceCollection();
            var starter = new Startup();
            starter.ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            starter.Configure(provider);
            starter.Run(provider);
        }
    }
}
