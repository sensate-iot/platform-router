/*
 * Data authorization context repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;

namespace SensateIoT.API.Common.Data.Dto.Authorization
{
	public class Sensor
	{
		public ObjectId Id { get; set; }
		public string Secret { get; set; }
		public string UserId { get; set; }
	}
}
