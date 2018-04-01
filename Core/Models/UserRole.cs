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
		public string Description { get; set; }
	}
}

