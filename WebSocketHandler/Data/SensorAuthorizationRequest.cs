/*
 * Authorize a sensor attempting to connect over a live websocket.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.WebSocketHandler.Data
{
	public class SensorAuthorizationRequest
	{
		public string SensorID { get; set; }
		public string SensorSecret { get; set; }
	}
}
