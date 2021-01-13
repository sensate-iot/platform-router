/*
 * Update handler for live data commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.Common.Caching.Realtime
{
	public class LiveDataRouteUpdateHandler
	{
		private readonly IDataCache m_cache;

		public LiveDataRouteUpdateHandler(IDataCache cache)
		{
			this.m_cache = cache;
		}

		public void HandleUpdate(Command cmd)
		{
			LiveDataRoute route;
			InternalLiveDataRoute @internal;

			switch(cmd.Cmd) {
			case CommandType.AddLiveDataSensor:
				@internal = JsonConvert.DeserializeObject<InternalLiveDataRoute>(cmd.Arguments);
				route = new LiveDataRoute {
					Target = @internal.Target,
					SensorID = ObjectId.Parse(@internal.SensorID)
				};

				this.m_cache.AddLiveDataRoute(route);
				break;

			case CommandType.RemoveLiveDataSensor:
				@internal = JsonConvert.DeserializeObject<InternalLiveDataRoute>(cmd.Arguments);
				route = new LiveDataRoute {
					Target = @internal.Target,
					SensorID = ObjectId.Parse(@internal.SensorID)
				};

				this.m_cache.RemoveLiveDataRoute(route);
				break;

			case CommandType.SyncLiveDataSensors:
				var raw = JsonConvert.DeserializeObject<InternalLiveDataSyncList>(cmd.Arguments);
				var list = raw.Sensors.Select(objId => new LiveDataRoute {
					SensorID = ObjectId.Parse(objId),
					Target = raw.Target
				});

				this.m_cache.SyncLiveData(list.ToList());
				break;

			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	internal class InternalLiveDataRoute
	{
		public string SensorID { get; set; }
		public string Target { get; set; }
	}

	internal class InternalLiveDataSyncList
	{
		public string Target { get; set; }
		public IEnumerable<string> Sensors { get; set; }
	}
}
