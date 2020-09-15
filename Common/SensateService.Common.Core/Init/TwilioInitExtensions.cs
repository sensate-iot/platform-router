/*
 * Dependency injection initialization extension methods for
 * Twillio's Text API.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.Extensions.DependencyInjection;

using Twilio;
using Twilio.Rest.Api.V2010.Account;

using SensateService.Common.Config.Config;
using SensateService.Common.Config.Settings;
using SensateService.Services;
using SensateService.Services.Adapters;

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
