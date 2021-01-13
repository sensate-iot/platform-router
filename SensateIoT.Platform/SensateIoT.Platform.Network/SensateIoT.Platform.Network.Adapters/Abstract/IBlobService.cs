/*
 * Blob service abstraction.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.Adapters.Abstract
{
	public interface IBlobService
	{
		Task StoreAsync(Blob blob, byte[] data, CancellationToken ct = default);
		Task<byte[]> ReadAsync(Blob blob, CancellationToken ct = default);
		Task RemoveAsync(Blob blob, CancellationToken ct = default);
		Task RemoveAsync(string sensorId, CancellationToken ct = default);
	}
}