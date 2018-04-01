/*
 * Measurement received event definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Events
{
	public static class MeasurementEvents
	{
		public static event OnMeasurementReceived MeasurementReceived;

		public async static Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e)
		{
			var handler = MeasurementReceived;

			if(handler == null)
				return;

			await handler(sender, e);
		}
	}
}
