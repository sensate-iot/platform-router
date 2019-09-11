/*
 * Bulk measurement processing event.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using SensateService.Models;

namespace SensateService.Infrastructure.Events
{
	public class MeasurementsReceivedEventArgs
	{
		public string Compressed { get; set; }
		public CancellationToken Token { get; }

		public MeasurementsReceivedEventArgs(CancellationToken token)
		{
			this.Token = token;
		}
	}
}
