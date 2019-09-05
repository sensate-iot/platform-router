/*
 * Bulk measurement processing event.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */


using System.Threading.Tasks;

namespace SensateService.Infrastructure.Events
{
	public delegate Task OnMeasurementsReceived(object sender, MeasurementsReceivedEventArgs args);
}
