/*
 * Standard (non-cached) measurement repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
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
	}
}
