/*
 * Live data update for the routing table.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class LiveDataRoute
	{
		public string Target { get; set; }
		public ObjectId SensorId { get; set; }

		public override int GetHashCode()
		{
			return this.Target.GetHashCode() ^ this.SensorId.GetHashCode();
		}
	}
}
