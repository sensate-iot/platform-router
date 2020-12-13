/*
 * User role mapping model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Newtonsoft.Json;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class UserRole
	{
		[JsonIgnore]
		public virtual User User { get; set; }
		public virtual Role Role { get; set; }
	}
}
