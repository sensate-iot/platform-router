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

using Microsoft.EntityFrameworkCore;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class LiveDataHandlerRepository : ILiveDataHandlerRepository
	{
		private readonly NetworkContext m_ctx;

		private const string Router_GetLiveDataHandlers = "router_getlivedatahandlers";

		public LiveDataHandlerRepository(NetworkContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<LiveDataHandler>> GetLiveDataHandlers(CancellationToken ct = default)
		{
			var result = new List<LiveDataHandler>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetLiveDataHandlers;

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var record = new LiveDataHandler {
							Name = reader.GetString(0),
							Enabled = true
						};

						result.Add(record);
					}
				}
			}

			return result;
		}
	}
}