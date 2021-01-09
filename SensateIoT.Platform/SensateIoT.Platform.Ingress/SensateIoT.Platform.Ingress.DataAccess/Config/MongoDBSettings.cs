/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Ingress.DataAccess.Config
{
	public class MongoDBSettings
	{
		public string ConnectionString { get; set; }
		public string DatabaseName { get; set; }
		public int MaxConnections { get; set; }
	}
}
