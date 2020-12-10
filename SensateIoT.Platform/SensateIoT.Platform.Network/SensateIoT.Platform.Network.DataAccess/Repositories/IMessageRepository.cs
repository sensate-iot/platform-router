/*
 * Message repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public interface IMessageRepository
	{
		Task CreateRangeAsync(IEnumerable<Message> messages, CancellationToken ct = default);
	}
}
