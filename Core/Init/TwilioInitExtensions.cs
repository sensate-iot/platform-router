/*
 * Dependency injection initialization extension methods for
 * Twillio's Text API.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.Extensions.DependencyInjection;
using SensateService.Config;
using SensateService.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SensateService.Init
{
	public static class TwilioInitExtensions
	{
		public static IServiceCollection AddTwilioTextApi(this IServiceCollection services, TextConfig config)
		{
			TwilioClient.Init(config.Twilio.AccountSid, config.Twilio.AuthToken);

			var incoming = IncomingPhoneNumberResource.Fetch(pathSid: config.Twilio.PhoneSid);
			services.Configure<TextServiceSettings>(options => {
				options.AlphaCode = config.AlphaCode;
				options.PhoneNumber = incoming.PhoneNumber.ToString();
			});

			services.AddScoped<ITextSendService, TwillioTextSendService>();
			return services;
		}
	}
}
