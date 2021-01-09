using System.Threading.Tasks;
using MongoDB.Bson;
using SensateIoT.Platform.Ingress.DataAccess.Models;

namespace SensateIoT.Platform.Ingress.DataAccess.Abstract
{
	public interface ISensorRepository
	{
		Task<Sensor> GetSensorAsync(ObjectId id);
	}
}
