/*
 * Dependency injectable MQTT publishing service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

namespace SensateService.Services.Processing
{
	public interface IMqttPublishService
	{
		Task PublishOnAsync(string topic, string message, bool retain);
		void PublishOn(string topic, string message, bool retain);
	}
}