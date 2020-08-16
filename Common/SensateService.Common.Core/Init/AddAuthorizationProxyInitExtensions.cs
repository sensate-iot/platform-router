/*
 * Authorization proxy initialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateService.Config;
using SensateService.Infrastructure.Authorization;
using SensateService.Services.Processing;

namespace SensateService.Init
{
	public static class AddAuthorizationProxyInitExtensions
	{
		public static IServiceCollection AddAuthorizationProxy(this IServiceCollection services, SystemConfig config)
		{
			services.AddSingleton(config);
			services.AddSingleton<IHostedService, AuthProxyService>();

			services.AddSingleton<IMeasurementAuthorizationProxyCache, AuthorizationProxyCache>();
			services.AddSingleton<IMessageAuthorizationProxyCache, AuthorizationProxyCache>();
			return services;
		}
	}
}