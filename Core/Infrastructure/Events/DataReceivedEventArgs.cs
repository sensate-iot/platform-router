/*
 * Bulk measurement processing event.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading;

namespace SensateService.Infrastructure.Events
{
	public class DataReceivedEventArgs
	{
		public string Compressed { get; set; }
		public CancellationToken Token { get; }

		public DataReceivedEventArgs(CancellationToken token)
		{
			this.Token = token;
		}
	}
}
