/*
 * Sensor aggregation service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Services.Processing
{
	public class SensorService : ISensorService
	{
		private readonly ISensorRepository m_sensors;
		private readonly ISensorLinkRepository m_links;

		public SensorService(ISensorRepository sensors, ISensorLinkRepository links)
		{
			this.m_links = links;
			this.m_sensors = sensors;
		}

		public async Task<IEnumerable<Sensor>> GetSensorsAsync(SensateUser user, string name, int skip = 0, int limit = 0, CancellationToken token = default)
		{
			var sensors = await this.GetSensorsAsync(user, skip, limit, token).AwaitBackground();
			var nameLower = name.ToLowerInvariant();
			return sensors.Where(x => x.Name.ToLowerInvariant().Contains(nameLower));
		}

		public async Task<IEnumerable<Sensor>> GetSensorsAsync(SensateUser user, int skip = 0, int limit = 0, CancellationToken token = default)
		{
			var worker = this.m_links.GetByUserAsync(user, token);
			var ownSensors = await this.m_sensors.GetAsync(user).AwaitBackground();
			var links = await worker.AwaitBackground();

			var sensorIds = links.Select(x => x.SensorId);
			var linkedSensors = (await this.m_sensors.GetAsync(sensorIds).AwaitBackground()).ToList();

			foreach(var linked in linkedSensors) {
				linked.Secret = "";
			}

			var rv = ownSensors.ToList();
			rv.AddRange(linkedSensors);

			if(skip > 0) {
				rv = rv.Skip(skip).ToList();
			}

			if(limit > 0) {
				rv = rv.Take(limit).ToList();
			}

			return rv;
		}
	}
}
