/*
 * Internal MQTT client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateIoT.Platform.Network.Common.MQTT
{
	public interface IInternalMqttClient
	{
		Task PublishOnAsync(string topic, string message, bool retain);
	}
}
