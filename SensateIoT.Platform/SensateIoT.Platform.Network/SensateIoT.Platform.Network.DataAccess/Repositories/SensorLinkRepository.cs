/*
 * Sensor link repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class SensorLinkRepository : ISensorLinkRepository
	{
		private readonly NetworkContext m_ctx;

		public SensorLinkRepository(NetworkContext context)
		{
			this.m_ctx = context;
		}

		public async Task CreateAsync(SensorLink link, CancellationToken token = default)
		{
			await this.m_ctx.SensorLinks.AddAsync(link, token).ConfigureAwait(false);
			await this.m_ctx.SaveChangesAsync(token).ConfigureAwait(false);
		}

		public async Task DeleteAsync(SensorLink link, CancellationToken token = default)
		{
			this.m_ctx.SensorLinks.Remove(link);
			await this.m_ctx.SaveChangesAsync(token).ConfigureAwait(false);
		}


		public async Task<IEnumerable<SensorLink>> GetAsync(string sensorId, CancellationToken ct = default)
		{
			var links = this.m_ctx.SensorLinks.Where(link => link.SensorId == sensorId);
			var rv = await links.ToListAsync(ct).ConfigureAwait(false);

			return rv;
		}

		public async Task<IEnumerable<SensorLink>> GetByUserAsync(string userId, CancellationToken token = default)
		{
			var results = this.m_ctx.SensorLinks.Where(x => x.UserId == userId);
			return await results.ToListAsync(token).ConfigureAwait(false);
		}

		public async Task<int> CountAsync(string userId, CancellationToken ct = default)
		{
			var query = this.m_ctx.SensorLinks.Where(x => x.UserId == userId);
			var count = await query.CountAsync(ct).ConfigureAwait(false);

			return count;
		}

		public async Task DeleteBySensorAsync(ObjectId sensor, CancellationToken ct = default)
		{
			var id = sensor.ToString();
			var query = this.m_ctx.SensorLinks.Where(x => x.SensorId == id);

			this.m_ctx.SensorLinks.RemoveRange(query);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}
	}
}
