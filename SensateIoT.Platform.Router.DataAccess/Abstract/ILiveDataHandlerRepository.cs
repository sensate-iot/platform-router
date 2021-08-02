/*
 * Live data handler repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateIoT.Platform.Router.Data.Models;

namespace SensateIoT.Platform.Router.DataAccess.Abstract
{
	public interface ILiveDataHandlerRepository
	{
		Task<IEnumerable<LiveDataHandler>> GetLiveDataHandlers(CancellationToken ct = default);
	}
}
