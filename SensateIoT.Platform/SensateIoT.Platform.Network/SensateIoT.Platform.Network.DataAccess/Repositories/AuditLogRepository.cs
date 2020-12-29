/*
 * Audit logging repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.ne
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using SensateIoT.Platform.Network.DataAccess.Extensions;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class AuditLogRepository : IAuditLogRepository
	{
		private readonly AuthorizationContext m_ctx;

		private const string NetworkApi_CreateAuditLog = "networkapi_createauditlog";

		public AuditLogRepository(AuthorizationContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task CreateAsync(AuditLog log, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithParameter("route", log.Route, NpgsqlDbType.Text);
			builder.WithParameter("method", (int) log.Method, NpgsqlDbType.Integer);
			builder.WithParameter("address", log.Address, NpgsqlDbType.Inet);
			builder.WithParameter("author", log.AuthorId, NpgsqlDbType.Text);
			builder.WithFunction(NetworkApi_CreateAuditLog);

			var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			await reader.DisposeAsync().ConfigureAwait(false);
		}
	}
}
