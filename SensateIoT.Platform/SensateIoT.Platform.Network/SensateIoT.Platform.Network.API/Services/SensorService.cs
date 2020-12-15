/*
 * Sensor aggregation service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.API.Services
{
	public class SensorService : ISensorService
	{
		private readonly ISensorRepository m_sensors;
		private readonly ISensorLinkRepository m_links;
		private readonly IApiKeyRepository m_apiKeys;
		private readonly IMeasurementRepository m_measurements;
		private readonly IMessageRepository m_messages;
		private readonly IControlMessageRepository m_control;
		private readonly ITriggerRepository m_triggers;

		public SensorService(
			ISensorRepository sensors,
			ISensorLinkRepository links,
			IMeasurementRepository measurements,
			ITriggerRepository triggers,
			IControlMessageRepository control,
			IMessageRepository messages,
			IApiKeyRepository keys
			)
		{
			this.m_links = links;
			this.m_sensors = sensors;
			this.m_control = control;
			this.m_apiKeys = keys;
			this.m_measurements = measurements;
			this.m_triggers = triggers;
			this.m_messages = messages;
		}

		public async Task<PaginationResult<Sensor>> GetSensorsAsync(User user, string name, int skip = 0, int limit = 0, CancellationToken token = default)
		{
			var sensors = await this.GetSensorsAsync(user, token: token).ConfigureAwait(false);
			var nameLower = name.ToLowerInvariant();
			var list = sensors.Values.ToList();

			list = list.Where(x => x.Name.ToLowerInvariant().Contains(nameLower)).ToList();
			sensors.Count = list.Count;

			if(skip > 0) {
				list = list.Skip(skip).ToList();
			}

			if(limit > 0) {
				list = list.Take(limit).ToList();
			}

			sensors.Values = list;

			return sensors;
		}

		public async Task DeleteAsync(Sensor sensor, CancellationToken ct = default)
		{
			var tasks = new[] {
				this.m_sensors.DeleteAsync(sensor.InternalId, ct),
				this.m_messages.DeleteBySensorId(sensor.InternalId, ct),
				this.m_control.DeleteBySensorAsync(sensor.InternalId, ct),
				this.m_measurements.DeleteBySensorId(sensor.InternalId, ct),
			};

			await this.m_links.DeleteBySensorAsync(sensor.InternalId, ct).ConfigureAwait(false);
			await this.m_triggers.DeleteBySensorAsync(sensor.InternalId.ToString(), ct);
			await this.m_apiKeys.DeleteAsync(sensor.Secret, ct).ConfigureAwait(false);
			await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		public async Task<PaginationResult<Sensor>> GetSensorsAsync(User user, int skip = 0, int limit = 0, CancellationToken token = default)
		{
			var worker = this.m_links.GetByUserAsync(user.ID.ToString(), token);
			var ownSensors = await this.m_sensors.GetAsync(user.ID).ConfigureAwait(false);
			var links = await worker.ConfigureAwait(false);

			var sensorIds = links.Select(x => x.SensorId);
			var linkedSensors = (await this.m_sensors.GetAsync(sensorIds).ConfigureAwait(false)).ToList();

			foreach(var linked in linkedSensors) {
				linked.Secret = "";
			}

			var rv = ownSensors.ToList();
			rv.AddRange(linkedSensors);
			var count = rv.Count;

			if(skip > 0) {
				rv = rv.Skip(skip).ToList();
			}

			if(limit > 0) {
				rv = rv.Take(limit).ToList();
			}

			return new PaginationResult<Sensor> {
				Count = count,
				Values = rv
			};
		}
	}
}
