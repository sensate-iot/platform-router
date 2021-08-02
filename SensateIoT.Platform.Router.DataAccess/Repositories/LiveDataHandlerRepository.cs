/*
 * Live data handler repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Router.Data.Models;
using SensateIoT.Platform.Router.DataAccess.Abstract;

namespace SensateIoT.Platform.Router.DataAccess.Repositories
{
	public class LiveDataHandlerRepository : ILiveDataHandlerRepository
	{
		private readonly INetworkingDbContext m_ctx;

		private const string Router_GetLiveDataHandlers = "router_getlivedatahandlers";

		public LiveDataHandlerRepository(INetworkingDbContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<LiveDataHandler>> GetLiveDataHandlers(CancellationToken ct = default)
		{
			var result = new List<LiveDataHandler>();

			await using var cmd = this.m_ctx.Connection.CreateCommand();

			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetLiveDataHandlers;

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			while(await reader.ReadAsync(ct)) {
				var record = new LiveDataHandler {
					Name = reader.GetString(0),
					Enabled = true
				};

				result.Add(record);
			}

			return result;
		}
	}
}