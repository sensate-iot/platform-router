/*
 * Remote storage queue.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Network.Common.Collections.Abstract
{
	public interface IRemoteStorageQueue
	{
		void Enqueue(IPlatformMessage message);
		Task FlushMessagesAsync();
	}
}
