/*
 * Remote public queue interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateIoT.Platform.Network.Common.Collections.Remote
{
	public interface IPublicRemoteQueue
	{
		void Enqueue(string data, string target);
		Task FlushQueueAsync();
	}
}
