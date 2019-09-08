/*
 * Measurement store implementation. The measurement store acts
 * as a write through storage controller, which means that data
 * isn't cached locally.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
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
		private readonly IUserRepository _users;
		private readonly ISensorStatisticsRepository _stats;

		public MeasurementStore(
			IUserRepository users,
			ISensorRepository sensors,
			IMeasurementRepository measurements,
			ISensorStatisticsRepository stats,
			IServiceProvider provider,
			ILogger<MeasurementStore> logger
		) : base(provider, logger)
		{
			this._sensors = sensors;
			this._measurements = measurements;
			this._users = users;
			this._stats = stats;
		}

		public override async Task StoreAsync(RawMeasurement obj, RequestMethod method)
		{
			Measurement m;
			Sensor sensor;
			SensateUser user;

			sensor = await this._sensors.GetAsync(obj.CreatedById).AwaitBackground();

			if(sensor == null)
				return;

			user = await this._users.GetAsync(sensor.Owner).AwaitBackground();

			if(user == null)
				return;

			var roles = await this._users.GetRolesAsync(user).AwaitBackground();

			if(!base.CanInsert(roles) || !base.InsertAllowed(user, obj.CreatedBySecret))
				return;

			m = base.ProcessRawMeasurement(sensor, obj);

			var measurement_worker = this._measurements.StoreAsync(sensor, m);
			var stats_worker = this._stats.IncrementAsync(sensor, method);
			var events_worker = InvokeMeasurementReceivedAsync(sensor, m);

			await Task.WhenAll(measurement_worker, stats_worker, events_worker).AwaitBackground();
		}

		private static async Task InvokeMeasurementReceivedAsync(Sensor sender, Measurement m)
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
