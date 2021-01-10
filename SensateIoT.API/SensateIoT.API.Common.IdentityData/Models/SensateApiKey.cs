/*
 * API key model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SensateIoT.API.Common.IdentityData.Enums;

namespace SensateIoT.API.Common.IdentityData.Models
{
	[Table("ApiKeys")]
	public class SensateApiKey
	{
		[Key]
		public string Id { get; set; }
		[Required]
		public string UserId { get; set; }
		[JsonIgnore]
		public virtual SensateUser User { get; set; }
		[Required]
		public string ApiKey { get; set; }
		[Required]
		public bool Revoked { get; set; }
		[Required]
		public DateTime CreatedOn { get; set; }
		[Required]
		public ApiKeyType Type { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
		public bool ReadOnly { get; set; }
		[Required]
		public long RequestCount { get; set; }
	}
}