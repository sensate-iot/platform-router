/*
 * User role mapping model.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace SensateService.Models
{
	public class SensateUserRole : IdentityUserRole<string>
	{
		[JsonIgnore]
		public virtual SensateUser User { get; set; }
		[JsonIgnore]
		public virtual SensateRole Role { get; set; }
	}
}
