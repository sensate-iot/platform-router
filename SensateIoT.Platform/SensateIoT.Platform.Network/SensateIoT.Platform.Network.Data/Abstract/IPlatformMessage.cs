/*
 * A routable platform message interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using MongoDB.Bson;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Data.Abstract
{
	public interface IPlatformMessage
	{
		ObjectId SensorID { get; }
		MessageType Type { get; }
		DateTime PlatformTimestamp { get; set; }

		bool Validate(Sensor sensor);
	}
}
