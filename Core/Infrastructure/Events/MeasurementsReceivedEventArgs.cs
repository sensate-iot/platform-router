/*
 * Bulk measurement processing event.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Threading;
using SensateService.Models;

namespace SensateService.Infrastructure.Events
{
	public class MeasurementsReceivedEventArgs
	{
		public IList<Measurement> Measurements { get; }
		public CancellationToken Token { get; }

		public MeasurementsReceivedEventArgs(IList<Measurement> measurements, CancellationToken token)
		{
			this.Measurements = measurements;
			this.Token = token;
		}
	}
}
