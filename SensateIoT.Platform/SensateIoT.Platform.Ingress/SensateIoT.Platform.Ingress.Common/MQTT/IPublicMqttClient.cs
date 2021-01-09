/*
 * Public MQTT publish service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

namespace SensateIoT.Platform.Ingress.Common.MQTT
{
	public interface IPublicMqttClient
	{
		Task PublishOnAsync(string topic, string message, bool retain);
	}
}
