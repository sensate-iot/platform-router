/*
 * Measurement event argument wrapper.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SensateService.Models.Database
{
	public delegate Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e);

	public class MeasurementReceivedEventArgs : EventArgs
	{
		public Measurement Measurement {get;set;}
	}
}
