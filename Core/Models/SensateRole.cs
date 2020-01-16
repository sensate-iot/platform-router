/*
 * Identity role model.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace SensateService.Models
{
	[Table("Roles")]
	public class SensateRole : IdentityRole
	{
		public const string Banned = "Banned";
		public const string Standard = "Users";
		public const string Administrator = "Administrators";

		public string Description { get; set; }
		[JsonIgnore]
		public ICollection<SensateUserRole> UserRoles { get; set; }
	}
}

