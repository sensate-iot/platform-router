/*
 * JSON model to update a users phone number.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class PhoneNumberUpdate
	{
		[Required]
		public string PhoneNumber { get; set; }
	}
}