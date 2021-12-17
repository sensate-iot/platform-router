/*
 * Remote public queue interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateIoT.Platform.Router.Common.Collections.Abstract
{
	public interface IPublicRemoteQueue
	{
		int QueueLength { get; }
		void Enqueue(string data, string target);
		Task FlushQueueAsync();
	}
}
