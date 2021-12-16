/*
 * Public MQTT publish service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

namespace SensateIoT.Platform.Router.Common.MQTT
{
	public interface IPublicMqttClient
	{
		bool IsConnected { get; }
		Task PublishOnAsync(string topic, string message, bool retain);
	}
}
