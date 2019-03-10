/*
 * User role mapping model.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class SensateUserRole : IdentityUserRole<string>
	{
		public virtual SensateUser User { get; set; }
		public virtual SensateRole Role { get; set; }
	}
}
