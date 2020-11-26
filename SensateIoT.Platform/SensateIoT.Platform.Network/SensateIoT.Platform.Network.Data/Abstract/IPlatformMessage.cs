/*
 * A routable platform message interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using MongoDB.Bson;

namespace SensateIoT.Platform.Network.Data.Abstract
{
	public interface IPlatformMessage
	{
		ObjectId SensorID { get; }
		MessageType Type { get; }
		DateTime PlatformTimestamp { get; set; }
	}
}
