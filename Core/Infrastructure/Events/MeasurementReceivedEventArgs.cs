/*
 * Measurement event argument wrapper.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;

using System.Threading;
using SensateService.Models;

namespace SensateService.Infrastructure.Events
{
	public class MeasurementReceivedEventArgs : EventArgs
	{
		public Measurement Measurement { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
