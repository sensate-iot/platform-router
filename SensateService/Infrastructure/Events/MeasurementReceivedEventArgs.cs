/*
 * Measurement event argument wrapper.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Events
{
	public delegate Task OnMeasurementReceived(object sender, MeasurementReceivedEventArgs e);

	public class MeasurementReceivedEventArgs : EventArgs
	{
		public Measurement Measurement {get;set;}
	}
}
