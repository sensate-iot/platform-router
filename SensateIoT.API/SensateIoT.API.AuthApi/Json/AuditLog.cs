/*
 * Audit log view model.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

using System;
using System.Net;

using Newtonsoft.Json;
using SensateIoT.API.Common.Core.Converters;
using SensateIoT.API.Common.Data.Enums;

namespace SensateService.Api.AuthApi.Json
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