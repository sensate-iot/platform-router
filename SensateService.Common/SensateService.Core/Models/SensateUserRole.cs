/*
 * User role mapping model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace SensateService.Models
{
	[Table("UserRoles")]
	public class SensateUserRole : IdentityUserRole<string>
	{
		[JsonIgnore]
		public virtual SensateUser User { get; set; }
		public virtual SensateRole Role { get; set; }
	}
}
