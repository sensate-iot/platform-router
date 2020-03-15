/*
 * Cryptography init extensions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.DependencyInjection;
using SensateService.Crypto;

namespace SensateService.Init
{
	public static class CryptoInitExtensions
	{
		public static IServiceCollection AddHashAlgorihms(this IServiceCollection services)
		{
			services.AddSingleton<IHashAlgorithm, SHA256Algorithm>();
			return services;
		}
	}
}