/*
 * Identity role model.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class SensateRole : IdentityRole
	{
		public const string Banned = "Banned";
		public const string Standard = "Users";
		public const string Administrator = "Administrators";

		public string Description { get; set; }
		public ICollection<SensateUserRole> UserRoles { get; set; }
	}
}

