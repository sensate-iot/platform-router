/*
 * Change phone number token bridge.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public class ChangePhoneNumberToken
	{
		[Key]
		public string IdentityToken { get; set; }
		[Key]
		public string PhoneNumber { get; set; }
		public string UserToken { get; set; }
		public SensateUser User { get; set; }
		public DateTime Timestamp { get; set; }
	}
}