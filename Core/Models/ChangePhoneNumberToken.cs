/*
 * Change phone number token bridge.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public class ChangePhoneNumberToken
	{
		[Key]
		public string IdentityToken { get; set; }
		public string PhoneNumber { get; set; }
		public string UserToken { get; set; }
	}
}