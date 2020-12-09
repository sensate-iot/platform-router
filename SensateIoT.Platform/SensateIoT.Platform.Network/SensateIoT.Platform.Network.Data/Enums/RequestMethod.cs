/*
 * Request method listing.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Network.StorageService.DTO
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
