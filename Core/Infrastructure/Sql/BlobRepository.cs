/*
 * Blob data access layer.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
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
	public class BlobRepository : AbstractSqlRepository<Blob>, IBlobRepository
	{
		public BlobRepository(SensateSqlContext context) : base(context)
		{
		}

		private async Task<IEnumerable<Blob>> FetchData(IQueryable<Blob> blobs, int skip, int limit, CancellationToken ct)
		{
			if(skip > 0) {
				blobs = blobs.Skip(skip);
			}

			if(limit > 0) {
				blobs = blobs.Take(limit);
			}

			var enumerated = await blobs.ToListAsync(ct).AwaitBackground();

			foreach(var blob in enumerated) {
				blob.Timestamp = DateTime.SpecifyKind(blob.Timestamp, DateTimeKind.Utc);
			}

			return enumerated;
		}


		public async Task<Blob> GetAsync(long blobId, CancellationToken ct = default)
		{
			var blob = await this.Data.FirstOrDefaultAsync(b => b.Id == blobId, ct).AwaitBackground();

			blob.Timestamp = DateTime.SpecifyKind(blob.Timestamp, DateTimeKind.Utc);
			return blob;
		}

		public Task<IEnumerable<Blob>> GetAsync(string sensorId, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			var blobs = this.Data.Where(blob => blob.SensorId == sensorId);
			return this.FetchData(blobs, skip, limit, ct);
		}

		public async Task<IEnumerable<Blob>> GetAsync(IList<Sensor> sensors,
													  DateTime start,
													  DateTime end,
													  int skip = -1,
													  int limit = -1,
													  CancellationToken ct = default)
		{
			var ids = sensors.Select(x => x.InternalId.ToString());
			var query = this.Data.Where(x => ids.Contains(x.SensorId) &&
											 x.Timestamp >= start &&
											 x.Timestamp <= end);

			if(skip > 0) {
				query = query.Skip(skip);
			}

			if(limit > 0) {
				query = query.Take(limit);
			}

			return await query.ToListAsync(ct).AwaitBackground();
		}

		public Task<IEnumerable<Blob>> GetLikeAsync(string sensorId, string fileName, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			var blobs = this.Data.Where(blob => blob.SensorId == sensorId && blob.FileName.Contains(fileName));
			return this.FetchData(blobs, skip, limit, ct);
		}

		public async Task<Blob> GetAsync(string sensorId, string fileName, CancellationToken ct = default)
		{
			var blobs = this.Data.Where(blob => blob.SensorId == sensorId && blob.FileName == fileName);
			var result = await blobs.FirstOrDefaultAsync(ct).AwaitBackground();

			result.Timestamp = DateTime.SpecifyKind(result.Timestamp, DateTimeKind.Utc);
			return result;
		}

		public async Task<bool> DeleteAsync(string sensorId, string fileName, CancellationToken ct = default)
		{
			var data = await this.Data.FirstOrDefaultAsync(
				blob => blob.SensorId == sensorId && blob.FileName == fileName,
					ct
					).AwaitBackground();

			if(data == null) {
				return false;
			}

			this.Data.Remove(data);
			await this.CommitAsync(ct).AwaitBackground();

			return true;
		}

		public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
		{
			var data = await this.Data.FirstOrDefaultAsync(blog => blog.Id == id, ct).AwaitBackground();

			if(data == null) {
				return false;
			}

			this.Data.Remove(data);
			await this.CommitAsync(ct).AwaitBackground();

			return true;
		}

		public async Task DeleteAsync(Sensor sensor, CancellationToken ct = default)
		{
			var id = sensor.ToString();
			var query = this.Data.Where(x => x.SensorId == id);

			this.Data.RemoveRange(query);
			await this.CommitAsync(ct).AwaitBackground();
		}
	}
}