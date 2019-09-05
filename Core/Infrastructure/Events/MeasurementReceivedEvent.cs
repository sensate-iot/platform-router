/*
 * Measurement received event definition.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Events
{
	public delegate Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e);
}
