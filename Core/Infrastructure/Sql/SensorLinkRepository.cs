/*
 * Sensor link repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class SensorLinkRepository : AbstractSqlRepository<SensorLink>, ISensorLinkRepository
	{
		public SensorLinkRepository(SensateSqlContext context) : base(context)
		{
		}

		public async Task DeleteAsync(SensorLink link, CancellationToken token = default)
		{
			this.Data.Remove(link);
			await this.CommitAsync(token).AwaitBackground();
		}

		public async Task DeleteForAsync(Sensor sensor, CancellationToken token = default)
		{
			var id = sensor.InternalId.ToString();
			var links = this.Data.Where(x => x.SensorId == id);

			this.Data.RemoveRange(links);
			await this.CommitAsync(token).AwaitBackground();
		}

		public async Task<IEnumerable<SensorLink>> GetAsync(string userId, string sensorId, CancellationToken ct = default)
		{
			var links = this.Data.Where(link => link.SensorId == sensorId || link.UserId == userId);
			var rv = await links.ToListAsync(ct).AwaitBackground();

			return rv;
		}

		public async Task<IEnumerable<SensorLink>> GetAsync(string userId, CancellationToken ct = default)
		{
			var links = this.Data.Where(link => link.UserId == userId);
			var rv = await links.ToListAsync(ct).AwaitBackground();

			return rv;
		}

		public async Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default)
		{
			var results = this.Data.Where(x => x.UserId == user.Id);
			return await results.ToListAsync(token).AwaitBackground();
		}
	}
}