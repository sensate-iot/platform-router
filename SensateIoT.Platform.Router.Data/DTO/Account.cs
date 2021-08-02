/*
 * Account DTO model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Router.Data.DTO
{
	public class Account
	{
		public Guid ID { get; set; }
		public bool HasBillingLockout { get; set; }
		public bool IsBanned { get; set; }
	}
}
