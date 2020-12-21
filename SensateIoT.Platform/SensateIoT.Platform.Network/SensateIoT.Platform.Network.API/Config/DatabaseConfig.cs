/*
 * Sensate database configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Network.API.Config
{
	public class DatabaseConfig
	{
		public MongoDBConfig MongoDB { get; set; }
		public PgSQLConfig SensateIoT { get; set; }
		public PgSQLConfig Networking { get; set; }
	}

	public class MongoDBConfig
	{
		public string DatabaseName { get; set; }
		public string ConnectionString { get; set; }
		public int MaxConnections { get; set; }
	}

	public class PgSQLConfig
	{
		public string ConnectionString { get; set; }
	}
}
