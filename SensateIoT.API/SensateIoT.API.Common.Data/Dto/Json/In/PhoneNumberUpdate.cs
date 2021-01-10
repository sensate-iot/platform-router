/*
 * JSON model to update a users phone number.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;

namespace SensateIoT.API.Common.Data.Dto.Json.In
{
	public class PhoneNumberUpdate
	{
		[Required]
		public string PhoneNumber { get; set; }
	}
}