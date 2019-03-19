/*
 * MongoDB settings.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

namespace SensateService.Infrastructure.Document
{
	public class MongoDBSettings
	{
		public string ConnectionString { get; set; }
		public string DatabaseName { get; set; }
		public int MaxConnections { get; set; }
	}
}
