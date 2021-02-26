using System;
using System.Data.Common;

using Microsoft.Extensions.Options;

using Npgsql;

using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.DataAccess.Contexts
{
	public class AuthorizationDbContext : IAuthorizationDbContext
	{
		private readonly NpgsqlConnection m_connection;

		public AuthorizationDbContext(IOptions<DatabaseSettings> settings)
		{
			this.m_connection = new NpgsqlConnection(settings.Value.AuthorizationDbConnectionString);
		}

		public DbConnection Connection => this.m_connection;

		public void Dispose()
		{
			this.m_connection.Dispose();
			GC.SuppressFinalize(this);
		}

	}
}