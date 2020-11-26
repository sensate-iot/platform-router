/*
 * Internal MQTT client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateIoT.Platform.Network.Common.Infrastructure
{
	public interface IInternalMqttClient
	{
		Task PublishOnAsync(string topic, string message, bool retain);
	}
}
