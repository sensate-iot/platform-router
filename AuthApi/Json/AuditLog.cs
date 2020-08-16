/*
 * Audit log view model.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

using System;
using System.Net;

using Newtonsoft.Json;

using SensateService.Common.Data.Enums;
using SensateService.Converters;

namespace SensateService.AuthApi.Json
{
	public class AuditLog
	{
		public long Id { get; set; }
		public string Route { get; set; }
		public RequestMethod Method { get; set; }
		[JsonConverter(typeof(IPAddressJsonConverter))]
		public IPAddress Address { get; set; }
		public string Email { get; set; }
		public DateTime Timestamp { get; set; }
	}
}