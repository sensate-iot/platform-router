/*
 * Request method listing.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Common.Data.Enums
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
