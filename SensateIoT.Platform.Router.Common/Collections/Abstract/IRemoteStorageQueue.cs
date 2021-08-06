/*
 * Remote storage queue.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateIoT.Platform.Router.Data.Abstract;

namespace SensateIoT.Platform.Router.Common.Collections.Abstract
{
	public interface IRemoteStorageQueue
	{
		void Enqueue(IPlatformMessage message);
		Task FlushMessagesAsync();
	}
}
