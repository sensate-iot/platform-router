/*
 * Link sensors and users.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Newtonsoft.Json;

namespace SensateIoT.API.Common.Data.Models
{
	public class SensorLink
	{
		[JsonRequired]
		public string SensorId { get; set; }
		[JsonRequired]
		public string UserId { get; set; }
	}
}