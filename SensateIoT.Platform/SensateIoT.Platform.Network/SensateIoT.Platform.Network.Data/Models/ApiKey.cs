/*
 * API key model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Newtonsoft.Json;
using SensateService.Common.IdentityData.Enums;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class ApiKey 
	{
		[JsonIgnore]
		public virtual User User { get; set; }
		public Guid UserId { get; set; }
		public string Key { get; set; }
		public bool Revoked { get; set; }
		public ApiKeyType Type { get; set; }
		public bool ReadOnly { get; set; }
	}
}
