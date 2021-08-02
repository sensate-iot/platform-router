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

using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Data.DTO;
using SensateIoT.Platform.Router.Data.Enums;

namespace SensateIoT.Platform.Router.Common.Caching.Routing
{
	public class LiveDataRouteUpdateHandler
	{
		private readonly IRoutingCache m_cache;

		public LiveDataRouteUpdateHandler(IRoutingCache cache)
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

				this.m_cache.SyncLiveDataRoutes(list.ToList());
				break;

			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	internal class InternalLiveDataRoute
	{
		public InternalLiveDataRoute(string sensorId, string target)
		{
			this.SensorID = sensorId;
			this.Target = target;
		}

		public string SensorID { get; }
		public string Target { get; }
	}

	internal class InternalLiveDataSyncList
	{
		public InternalLiveDataSyncList(IEnumerable<string> sensors, string target)
		{
			this.Sensors = sensors;
			this.Target = target;
		}

		public string Target { get; }
		public IEnumerable<string> Sensors { get; }
	}
}
