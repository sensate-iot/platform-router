/*
 * JSON view for admin lookups.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateService.Common.Data.Dto.Json.Out;

namespace SensateService.AuthApi.Json
{
	public class AdminUserView : User
	{
		public string UnconfirmedPhoneNumber { get; set; }
		public bool BillingLockout { get; set; }
		public DateTime? PasswordLockout { get; set; }
		public bool PasswordLockoutEnabled { get; set; }
		public bool EmailConfirmed { get; set; }
	}
}