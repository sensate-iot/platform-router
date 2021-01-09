/*
 * Sensor link repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NpgsqlTypes;

using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;
using SensateService.Infrastructure.Extensions;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure.Sql
{
	public class SensorLinkRepository : ISensorLinkRepository
	{
		private const string DataApi_GetByUserID = "dataapi_selectsensorlinkbyuserid";
		private const string DataApi_Count = "dataapi_selectsensorlinkcountbyuserid";

		private readonly NetworkContext m_ctx;

		public SensorLinkRepository(NetworkContext context)
		{
			this.m_ctx = context;
		}

		public async Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(DataApi_GetByUserID);
			builder.WithParameter("userid", Guid.Parse(user.Id), NpgsqlDbType.Uuid);
			await using var reader = await builder.ExecuteAsync(token).ConfigureAwait(false);

			var list = new List<SensorLink>();

			while(await reader.ReadAsync(token).ConfigureAwait(false)) {
				var link = new SensorLink {
					SensorId = reader.GetString(0),
					UserId = reader.GetGuid(1).ToString()
				};

				list.Add(link);
			}

			return list;
		}

		public async Task<long> CountAsync(SensateUser user, CancellationToken token = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(DataApi_Count);
			builder.WithParameter("userid", Guid.Parse(user.Id), NpgsqlDbType.Uuid);
			await using var reader = await builder.ExecuteAsync(token).ConfigureAwait(false);

			if(!await reader.ReadAsync(token).ConfigureAwait(false)) {
				return 0;
			}

			return reader.GetInt64(0);
		}
	}
}