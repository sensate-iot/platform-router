/*
 * Email change (JSON) viewmodel.
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Common.Data.Dto.Json.In
{
	public class UpdateEmail
	{
		[Required]
		public string NewEmail { get; set; }
	}

	public class ConfirmUpdateEmail
	{
		[Required]
		public string Token { get; set; }
	}
}
