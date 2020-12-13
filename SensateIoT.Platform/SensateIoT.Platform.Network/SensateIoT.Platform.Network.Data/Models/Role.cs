/*
 * Identity role model.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class Role
	{
		public const string Banned = "Banned";
		public const string Standard = "Users";
		public const string Administrator = "Administrators";

		public string NormalizedName { get; set; }
		public string Description { get; set; }

		[JsonIgnore]
		public ICollection<UserRole> UserRoles { get; set; }
	}
}

