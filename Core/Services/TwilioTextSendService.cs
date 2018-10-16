/*
 * SMS sender service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Messaging.V1.Service;
using Twilio.Types;
using PhoneNumberResource = Twilio.Rest.Lookups.V1.PhoneNumberResource;

namespace SensateService.Services
{
	public class TwilioTextSendService : ITextSendService
	{
		public async Task SendAsync(string id, string to, string body)
		{
			throw new System.NotImplementedException();
		}

		public void Send(string id, string to, string body)
		{
			var msg = MessageResource.Create(
				new PhoneNumber(to),
				from: new PhoneNumber(id),
				body: body
			);
		}

		public async Task<bool> IsValidNumber(string number)
		{
			var verified = await Task.Run(() => {
				var num = PhoneNumberResource.Fetch(
					pathPhoneNumber: new PhoneNumber(number)
				);
				return num;
			});

			return verified != null;
		}
	}
}