/*
 * Live data update for the routing table.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;

namespace SensateIoT.Platform.Router.Data.DTO
{
	public class LiveDataRoute
	{
		public string Target { get; set; }
		public ObjectId SensorID { get; set; }

		public override int GetHashCode()
		{
			return this.Target.GetHashCode() ^ this.SensorID.GetHashCode();
		}
	}
}
