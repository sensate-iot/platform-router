/*
 * Request method listing.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Enums
{
	public enum RequestMethod
	{
		HttpGet,
		HttpPost,
		HttpPatch,
		HttpPut,
		HttpDelete,

		WebSocket,

		MqttTcp,
		MqttWebSocket,
		Any
	}
}
