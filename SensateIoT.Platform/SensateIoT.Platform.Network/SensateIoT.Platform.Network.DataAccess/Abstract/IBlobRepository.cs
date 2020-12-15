using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IBlobRepository
	{
		Task DeleteAsync(ObjectId sensor, CancellationToken ct = default);
	}
}