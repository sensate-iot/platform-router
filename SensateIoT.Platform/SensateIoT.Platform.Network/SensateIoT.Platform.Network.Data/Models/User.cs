/*
 * Sensate user model.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class User
	{
		[Required]
		public string FirstName { get; set; }
		[Required]
		public string LastName { get; set; }
		public string UnconfirmedPhoneNumber { get; set; }
		[Required]
		public DateTime RegisteredAt { get; set; }
		[Required]
		public bool BillingLockout { get; set; }
		public virtual ICollection<UserRole> UserRoles { get; set; }
		public virtual ICollection<ApiKey> ApiKeys { get; set; }
	}
}
