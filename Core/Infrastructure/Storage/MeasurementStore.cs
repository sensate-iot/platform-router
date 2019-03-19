/*
 * Measurement store implementation. The measurement store acts
 * as a write through storage controller, which means that data
 * isn't cached locally.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	public class MeasurementStore : AbstractMeasurementStore
	{
		public static event OnMeasurementReceived MeasurementReceived;

		private readonly ISensorRepository _sensors;
		private readonly IMeasurementRepository _measurements;
		private readonly IAuditLogRepository _logs;
		private readonly IUserRepository _users;
		private readonly ISensorStatisticsRepository _stats;

		public MeasurementStore(
			IUserRepository users,
			ISensorRepository sensors,
			IMeasurementRepository measurements,
			IAuditLogRepository logs,
			ISensorStatisticsRepository stats,
			IServiceProvider provider,
			ILogger<MeasurementStore> logger
		) : base(provider, logger)
		{
			this._sensors = sensors;
			this._measurements = measurements;
			this._logs = logs;
			this._users = users;
			this._stats = stats;
		}

		public override async Task StoreAsync(RawMeasurement obj, RequestMethod method)
		{
			Measurement m;
			AuditLog log;
			Sensor sensor;
			SensateUser user;

			sensor = await this._sensors.GetAsync(obj.CreatedById).AwaitBackground();

			if(sensor == null)
				return;

			user = await this._users.GetAsync(sensor.Owner).AwaitBackground();

			if(user == null)
				return;

			var worker = this._users.GetRolesAsync(user);

			log = new AuditLog {
				Address = IPAddress.Any,
				Method = method,
				Route = "sensate/measurements/rt/new",
				Timestamp = DateTime.Now,
				AuthorId = user.Id
			};

			var roles = await worker.AwaitBackground();
			var auditworker = this._logs.CreateAsync(log);

			if(!base.CanInsert(roles)) {
				await auditworker.AwaitBackground();
				return;
			}

			m = base.ProcessRawMeasurement(sensor, obj);

			var measurement_worker = this._measurements.CreateAsync(m);
			var stats_worker = this._stats.IncrementAsync(sensor);
			var events_worker = InvokeMeasurementReceivedAsync(sensor, m);

			await Task.WhenAll(auditworker, measurement_worker, stats_worker, events_worker).AwaitBackground();
		}

		private static async Task InvokeMeasurementReceivedAsync(object sender, Measurement m)
		{
			Delegate[] delegates;
			MeasurementReceivedEventArgs args;

			if(MeasurementReceived == null)
				return;

			delegates = MeasurementReceived.GetInvocationList();

			if(delegates.Length <= 0)
				return;

			args = new MeasurementReceivedEventArgs {
				CancellationToken = CancellationToken.None,
				Measurement = m
			};

			await MeasurementReceived.Invoke(sender, args).AwaitBackground();
		}
	}
}
