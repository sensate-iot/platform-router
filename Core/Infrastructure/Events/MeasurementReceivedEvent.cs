/*
 * Measurement received event definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Events
{
	public delegate Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e);
}
