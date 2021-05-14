using System;
using System.Data.Common;

using Microsoft.Extensions.Options;
using Npgsql;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Settings;

namespace SensateIoT.Platform.Network.DataAccess.Contexts
{
	public class NetworkingDbContext : INetworkingDbContext
	{
		private readonly NpgsqlConnection m_connection;

		public NetworkingDbContext(IOptions<DatabaseSettings> settings)
		{
			this.m_connection = new NpgsqlConnection(settings.Value.NetworkingDbConnectionString);
		}

		public DbConnection Connection => this.m_connection;

		public void Dispose()
		{
			this.m_connection.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}