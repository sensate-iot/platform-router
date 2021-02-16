/*
 * Sensate IoT database configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Network.TriggerService.Config
{
	public class DatabaseConfig
	{
		public PgSQLConfig SensateIoT { get; set; }
		public MongoDBConfig MongoDB { get; set; }
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
