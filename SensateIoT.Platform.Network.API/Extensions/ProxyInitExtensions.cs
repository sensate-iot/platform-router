using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace SensateIoT.Platform.Network.API.Extensions
{
	public static class ProxyInitExtensions
	{
		public static IServiceCollection AddReverseProxy(this IServiceCollection services, int level)
		{
			services.Configure<ForwardedHeadersOptions>(options => {
				options.ForwardLimit = level;

				options.KnownProxies.Clear();
				options.KnownNetworks.Clear();
				options.ForwardedHeaders = ForwardedHeaders.All;
			});

			return services;
		}
	}
}
