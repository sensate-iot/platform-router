/*
 * Dependency injection initialization extension methods for
 * Twillio's Text API.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.Extensions.DependencyInjection;
using SensateService.Services;
using Twilio;

namespace SensateService.Init
{
	public static class TwilioInitExtensions
	{
		public static IServiceCollection AddTwilioTextApi(this IServiceCollection services, string sid, string token)
		{
			TwilioClient.Init(sid, token);
			services.AddScoped<ITextSendService, TwilioTextSendService>();

			return services;
		}
	}
}
