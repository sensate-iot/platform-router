/*
 * API key model.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SensateService.Enums;

namespace SensateService.Models
{
	[Table("AspNetApiKeys")]
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
	}
}