/*
 * Audit logging model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.ne
 */

using System;
using System.Net;

using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class AuditLog
	{
		public long Id { get; set; }
		public string Route { get; set; }
		public RequestMethod Method { get; set; }
		public IPAddress Address { get; set; }
		public string AuthorId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
