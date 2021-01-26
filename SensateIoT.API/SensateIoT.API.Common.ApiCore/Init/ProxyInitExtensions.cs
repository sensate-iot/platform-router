/*
 * Extension class to setup a reverse
 * proxy API configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using SensateIoT.API.Common.Config.Config;

namespace SensateIoT.API.Common.ApiCore.Init
{
	public static class ProxyInitExtensions
	{
		public static IServiceCollection AddReverseProxy(this IServiceCollection services, SystemConfig conf)
		{
			services.Configure<ForwardedHeadersOptions>(options => {
				options.ForwardLimit = conf.ProxyLevel;

				options.KnownProxies.Clear();
				options.KnownNetworks.Clear();
				options.ForwardedHeaders = ForwardedHeaders.All;
			});

			return services;
		}
	}
}
