/*
 * Cryptography init extensions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.DependencyInjection;
using SensateIoT.API.Common.Core.Crypto;

namespace SensateIoT.API.Common.Core.Init
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