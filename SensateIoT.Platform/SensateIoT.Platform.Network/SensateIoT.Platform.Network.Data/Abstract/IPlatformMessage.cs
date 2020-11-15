/*
 * A routable platform message interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Data.Abstract
{
	public interface IPlatformMessage
	{
		ObjectId SensorID { get; }
		MessageType Type { get; }

		bool Validate(Sensor sensor);
	}
}
