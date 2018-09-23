/*
 * Measurement received event definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading.Tasks;

namespace SensateService.Infrastructure.Events
{
	public delegate Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e);

	public static class MeasurementEvents
	{
		public static event OnMeasurementReceived MeasurementReceived;


		public static async Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e)
		{
			if(MeasurementReceived == null)
				return;

			if (MeasurementReceived.GetInvocationList().Length <= 0)
				return;

			if (MeasurementReceived.GetInvocationList().Length > 2) {
				Console.WriteLine("WTF happend!");
				return;
			}

			await MeasurementReceived.Invoke(sender, e);
		}
	}
}
