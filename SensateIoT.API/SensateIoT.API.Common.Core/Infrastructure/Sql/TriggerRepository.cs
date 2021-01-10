/*
 * Trigger data layer implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using NpgsqlTypes;
using SensateIoT.API.Common.Core.Infrastructure.Extensions;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;

namespace SensateIoT.API.Common.Core.Infrastructure.Sql
{
	public class TriggerRepository : ITriggerRepository
	{
		private const string DataApi_Count = "dataapi_selectinvocationcount";

		private readonly NetworkContext m_ctx;

		public TriggerRepository(NetworkContext context)
		{
			this.m_ctx = context;
		}

		public async Task<long> CountAsync(IEnumerable<ObjectId> sensorIds, CancellationToken token)
		{
			var ids = sensorIds.Select(x => x.ToString());
			var idlist = string.Join(',', ids);
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(DataApi_Count);
			builder.WithParameter("idlist", idlist, NpgsqlDbType.Text);
			await using var reader = await builder.ExecuteAsync(token).ConfigureAwait(false);

			if(!await reader.ReadAsync(token).ConfigureAwait(false)) {
				return 0;
			}

			return reader.GetInt64(0);
		}
	}
}
