/*
 * Sensor aggregation service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.Services.Processing
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
		private readonly IBlobRepository m_blobs;

		public SensorService(
			ISensorRepository sensors,
			ISensorLinkRepository links,
			IMeasurementRepository measurements,
			ITriggerRepository triggers,
			IControlMessageRepository control,
			IMessageRepository messages,
			IApiKeyRepository keys,
			IBlobRepository blobs
			)
		{
			this.m_links = links;
			this.m_blobs = blobs;
			this.m_sensors = sensors;
			this.m_control = control;
			this.m_apiKeys = keys;
			this.m_measurements = measurements;
			this.m_triggers = triggers;
			this.m_messages = messages;
		}

		public async Task<PaginationResult<Sensor>> GetSensorsAsync(SensateUser user, string name, int skip = 0, int limit = 0, CancellationToken token = default)
		{
			var sensors = await this.GetSensorsAsync(user, token: token).AwaitBackground();
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
				this.m_sensors.DeleteAsync(sensor, ct),
				this.m_messages.DeleteBySensorAsync(sensor, ct),
				this.m_control.DeleteBySensorAsync(sensor, ct),
				this.m_measurements.DeleteBySensorAsync(sensor, ct),
			};

			await this.m_links.DeleteBySensorAsync(sensor, ct).AwaitBackground();
			await this.m_triggers.DeleteBySensorAsync(sensor.InternalId.ToString(), ct);
			await this.m_blobs.DeleteAsync(sensor, ct).AwaitBackground();
			await this.m_apiKeys.DeleteAsync(sensor.Secret, ct).AwaitBackground();
			await Task.WhenAll(tasks).AwaitBackground();
		}

		public async Task<PaginationResult<Sensor>> GetSensorsAsync(SensateUser user, int skip = 0, int limit = 0, CancellationToken token = default)
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
