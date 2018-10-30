/*
 * Dependency injectable MQTT publishing service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;

namespace SensateService.Services
{
	public interface IMqttPublishService
	{
		Task PublishOnAsync(string topic, string message, bool retain);
		void PublishOn(string topic, string message, bool retain);
	}
}