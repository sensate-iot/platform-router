/*
 * Authorization proxy initialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SensateIoT.API.Common.Config.Config;
using SensateIoT.API.Common.Core.Infrastructure.Authorization;
using SensateIoT.API.Common.Core.Services.Processing;

namespace SensateIoT.API.Common.Core.Init
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