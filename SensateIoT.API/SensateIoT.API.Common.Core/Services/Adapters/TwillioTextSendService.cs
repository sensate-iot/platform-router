/*
 * SMS sender service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensateIoT.API.Common.Config.Settings;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using PhoneNumberResource = Twilio.Rest.Lookups.V1.PhoneNumberResource;

namespace SensateIoT.API.Common.Core.Services.Adapters
{
	public class TwillioTextSendService : ITextSendService
	{
		private readonly TextServiceSettings _settings;
		private readonly ILogger<TwillioTextSendService> _logger;

		private const int AlphaNumericNotSupportedCode = 21612;

		public TwillioTextSendService(IOptions<TextServiceSettings> settings, ILogger<TwillioTextSendService> logger)
		{
			this._settings = settings.Value;
			this._logger = logger;
		}

		public async Task SendAsync(string id, string to, string body, bool retry = true)
		{
			await Task.Run(() => { this.Send(id, to, body); });
		}

		public void Send(string id, string to, string body, bool retry = true)
		{
			try {
				MessageResource.Create(
					new PhoneNumber(to),
					@from: new PhoneNumber(id),
					body: body
				);
			} catch(ApiException ex) {
				if(ex.Code == AlphaNumericNotSupportedCode && retry) {
					this._logger.LogInformation("Unable to send message using alpha-numeric ID. Trying with phone number..");
					this.Send(this._settings.PhoneNumber, to, body, false);
				} else {
					this._logger.LogInformation($"Unable to send text message: {ex.Message}");
				}
			}
		}

		public async Task<bool> IsValidNumber(string number)
		{
			var verified = await Task.Run(() => {
				PhoneNumberResource res;

				try {
					res = PhoneNumberResource.Fetch(
						pathPhoneNumber: new PhoneNumber(number)
					);
				} catch(ApiException ex) {
					Debug.WriteLine(ex.Message);
					res = null;
				}

				return res;
			});

			return verified != null;
		}
	}
}