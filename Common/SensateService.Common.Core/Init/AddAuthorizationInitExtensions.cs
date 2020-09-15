/*
 * Authorization init extensions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateService.Infrastructure.Authorization;
using SensateService.Infrastructure.Authorization.Cache;
using SensateService.Infrastructure.Repositories;
using SensateService.Services.Processing;

namespace SensateService.Init
{
	public static class AddAuthorizationInitExtensions
	{
		public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
		{
			services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
			services.AddSingleton<IDataCache, DataCache>();
			services.AddSingleton<IAuthorizationCache, AuthorizationCache>();
			services.AddSingleton<IHostedService, DataAuthorizationService>();

			return services;
		}
	}
}