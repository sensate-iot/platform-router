/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

namespace SensateService.Middleware
{
	public abstract class MqttHandler
	{
		public abstract void OnMessage(string topic, string msg);
		public abstract Task OnMessageAsync(string topic, string message);
	}
}
