/*
 * A routable platform message interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using JetBrains.Annotations;
using MongoDB.Bson;

namespace SensateIoT.Platform.Network.Data.Abstract
{
	[PublicAPI]
	public interface IPlatformMessage
	{
		ObjectId SensorID { get; }
		MessageType Type { get; }
		DateTime PlatformTimestamp { get; set; }
	}
}
