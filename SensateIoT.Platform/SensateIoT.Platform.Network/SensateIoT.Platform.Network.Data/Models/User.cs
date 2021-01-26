/*
 * Sensate user model.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class User
	{
		public Guid ID { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime RegisteredAt { get; set; }
		public bool BillingLockout { get; set; }
		public virtual ICollection<string> UserRoles { get; set; }
	}
}
