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
		public string Id { get; set; }
		public string AccountId { get; set; }
		[JsonIgnore]
		public virtual User User { get; set; }
		public string Key { get; set; }
		public bool Revoked { get; set; }
		public DateTime CreatedOn { get; set; }
		public ApiKeyType Type { get; set; }
		public string Name { get; set; }
		public bool ReadOnly { get; set; }
		public long RequestCount { get; set; }
	}
}