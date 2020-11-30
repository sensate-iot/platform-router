/*
 * Type safe in-memory sensor cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public interface ISensorCache : IMemoryCache<ObjectId, Sensor>
	{

		void AddLiveDataRouting(LiveDataRoute route);
		void RemoveLiveDataRouting(LiveDataRoute route);
		void SyncLiveDataRoutes(ICollection<LiveDataRoute> data);
		void FlushLiveDataRoutes();
	}
}
