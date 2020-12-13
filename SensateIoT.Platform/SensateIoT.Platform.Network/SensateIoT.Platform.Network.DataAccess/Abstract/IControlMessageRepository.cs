/*
 * Control message repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IControlMessageRepository
	{
		Task CreateAsync(ControlMessage obj, CancellationToken ct = default);
	}
}
