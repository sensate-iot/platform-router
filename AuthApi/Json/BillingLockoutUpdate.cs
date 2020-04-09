/*
 * Billing lockout update.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.AuthApi.Json
{
	public class BillingLockoutUpdate
	{
		[Required]
		public bool BillingLockout { get; set; }
	}
}