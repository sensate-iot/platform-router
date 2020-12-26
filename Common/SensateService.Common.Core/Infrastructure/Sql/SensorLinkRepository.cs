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
	}
}