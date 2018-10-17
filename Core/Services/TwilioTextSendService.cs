/*
 * SMS sender service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

using PhoneNumberResource = Twilio.Rest.Lookups.V1.PhoneNumberResource;

namespace SensateService.Services
{
	public class TwilioTextSendService : ITextSendService
	{
		public async Task SendAsync(string id, string to, string body)
		{
			await Task.Run(() => { this.Send(id, to, body); });
		}

		public void Send(string id, string to, string body)
		{
			try {
				MessageResource.Create(
					new PhoneNumber(to),
					from: new PhoneNumber(id),
					body: body
				);
			} catch(Exception ex) {
				Debug.WriteLine(ex.Message);
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