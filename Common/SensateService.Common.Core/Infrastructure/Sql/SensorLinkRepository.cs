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

using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;

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

		public async Task<IEnumerable<SensorLink>> GetAsync(string sensorId, CancellationToken ct = default)
		{
			var links = this.Data.Where(link => link.SensorId == sensorId);
			var rv = await links.ToListAsync(ct).AwaitBackground();

			return rv;
		}

		public async Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default)
		{
			if(user == null) {
				return null;
			}

			var results = this.Data.Where(x => x.UserId == user.Id);
			return await results.ToListAsync(token).AwaitBackground();
		}

		public async Task<int> CountAsync(SensateUser user, CancellationToken ct = default)
		{
			var query = this.Data.Where(x => x.UserId == user.Id);
			var count = await query.CountAsync(ct).AwaitBackground();

			return count;
		}

		public async Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			var id = sensor.InternalId.ToString();
			var query = this.Data.Where(x => x.SensorId == id);

			this.Data.RemoveRange(query);
			await this.CommitAsync(ct).AwaitBackground();
		}

		public async Task DeleteByUserAsync(SensateUser user, CancellationToken ct = default)
		{
			var query = this.Data.Where(x => x.UserId == user.Id);

			this.Data.RemoveRange(query);
			await this.CommitAsync(ct).AwaitBackground();
		}
	}
}