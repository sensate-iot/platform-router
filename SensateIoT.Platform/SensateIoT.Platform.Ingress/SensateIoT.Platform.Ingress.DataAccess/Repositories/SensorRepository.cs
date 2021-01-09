using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using SensateIoT.Platform.Ingress.DataAccess.Abstract;
using SensateIoT.Platform.Ingress.DataAccess.Contexts;
using SensateIoT.Platform.Ingress.DataAccess.Models;

namespace SensateIoT.Platform.Ingress.DataAccess.Repositories
{
	public class SensorRepository : ISensorRepository
	{
		private readonly IMongoCollection<Sensor> m_sensors;

		public SensorRepository(MongoDBContext ctx)
		{
			this.m_sensors = ctx.Sensors;
		}

		public async Task<SensorSecret> GetSensorAsync(ObjectId id)
		{
			return await this.m_sensors
				.Find(s => s.Id == id)
				.Project(s => new SensorSecret {
					Id = s.Id,
					Secret = s.Secret
				})
				.FirstOrDefaultAsync();
		}
	}
}