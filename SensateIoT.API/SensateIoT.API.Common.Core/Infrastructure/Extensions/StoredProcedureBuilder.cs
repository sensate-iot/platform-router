/*
 * Build a stored procedure execution context.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace SensateIoT.API.Common.Core.Infrastructure.Extensions
{
	public class StoredProcedureBuilder : IDisposable
	{
		private readonly DbCommand m_cmd;
		private bool m_disposed;

		internal StoredProcedureBuilder(DbConnection connection)
		{
			this.m_disposed = false;
			this.m_cmd = connection.CreateCommand();
		}

		public void WithParameter(string name, object value, NpgsqlDbType type)
		{
			var npgsql = new NpgsqlParameter(name, type) { Value = value ?? DBNull.Value };
			this.m_cmd.Parameters.Add(npgsql);
		}

		public async Task<DbDataReader> ExecuteAsync(CancellationToken ct = default)
		{
			if(this.m_cmd.Connection.State != ConnectionState.Open) {
				await this.m_cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			return await this.m_cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
		}

		public void WithFunction(string name)
		{
			this.m_cmd.CommandText = name;
			this.m_cmd.CommandType = CommandType.StoredProcedure;
		}

		public static StoredProcedureBuilder Create(DbConnection connection)
		{
			return new StoredProcedureBuilder(connection);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(this.m_disposed) {
				return;
			}

			if(disposing) {
				this.m_cmd.Dispose();
			}

			this.m_disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}