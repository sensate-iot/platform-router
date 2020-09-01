/*
 * Dependency injectable MQTT publishing service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SensateService.Services
{
	public interface IMqttPublishService : IHostedService
	{
		Task PublishOnAsync(string topic, string message, bool retain);
		void PublishOn(string topic, string message, bool retain);
	}
}