/*
 * Standard (non-cached) measurement repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using SensateService.Models.Repositories;

namespace SensateService.Models.Database
{
	public class StandardMeasurementRepository : AbstractMeasurementRepository, IMeasurementRepository
	{
		public StandardMeasurementRepository(
			SensateContext context,
			ILogger<AbstractMeasurementRepository> logger
		) : base(context, logger)
		{

		}

		public override void Commit(Measurement obj)
		{
			return;
		}

		public async override Task CommitAsync(Measurement obj)
		{
			await Task.CompletedTask;
		}

		public Measurement GetMeasurement(string key, Expression<Func<Measurement, bool>> selector)
		{
			return base.TryGetMeasurement(key, selector);
		}

		public async Task<Measurement> GetMeasurementAsync(string key, Expression<Func<Measurement, bool>> selector)
		{
			return await base.TryGetMeasurementAsync(key, selector);
		}

		public IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit)
		{
			return this.TryGetMeasurements(null, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) <= 0
			);
		}

		public IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit)
		{
			return this.TryGetMeasurements(null, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);
		}

		public async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit)
		{
			return await this.TryGetMeasurementsAsync(null, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) <= 0
			);
		}

		public async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit)
		{
			return await this.TryGetMeasurementsAsync(null, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);
		}
	}
}