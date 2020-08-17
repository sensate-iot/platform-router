/*
 * Measurement event argument wrapper.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;

using System.Threading;
using SensateService.Common.Data.Models;

namespace SensateService.Infrastructure.Events
{
	public class MeasurementReceivedEventArgs : EventArgs
	{
		public Measurement Measurement { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public Sensor Sensor { get; set; }
	}
}
