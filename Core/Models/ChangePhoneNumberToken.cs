/*
 * Change phone number token bridge.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensateService.Models
{
	[Table("AspNetPhoneNumberTokens")]
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