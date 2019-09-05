/*
 * Sensate user model.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class SensateUser : IdentityUser
	{
		[Required]
		public string FirstName { get; set; }
		[Required]
		public string LastName { get; set; }
		public string UnconfirmedPhoneNumber { get; set; }
		[Required]
		public DateTime RegisteredAt { get; set; }
		public virtual ICollection<SensateUserRole> UserRoles { get; set; }
		public virtual ICollection<SensateApiKey> ApiKeys { get; set; }
	}
}
