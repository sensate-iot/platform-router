/*
 * MQTT handler interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.Platform.Ingress.Common.MQTT
{
	public interface IMqttHandler
	{
		Task OnMessageAsync(string topic, string message, CancellationToken ct = default);
	}
}
