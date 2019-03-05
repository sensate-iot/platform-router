/*
 * Identity role model.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class UserRole : IdentityRole
	{
		public const string Banned = "Banned";
		public const string Standard = "Users";
		public const string Administrator = "Administrators";

		public string Description { get; set; }
	}
}

