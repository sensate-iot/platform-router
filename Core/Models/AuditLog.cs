/*
 * Sensor meta data model
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Net;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Converters;

namespace SensateService.Models
{
	public class AuditLog
	{
		[Required, Key]
		public long Id { get; set; }

		[Required]
		public string Route { get; set; }

		[Required]
		public RequestMethod Method { get; set; }

		[Required, JsonConverter(typeof(IPAddressJsonConverter))]
		public IPAddress Address { get; set; }

		public string AuthorId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}