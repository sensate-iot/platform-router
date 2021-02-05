/*
 * Convert to and from message database models.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.Common.Converters
{
	public static class MessageDatabaseConverter
	{
		public static IEnumerable<Message> Convert(TextMessageData messages)
		{
			return messages.Messages.Select(msg => new Message {
				Data = msg.Data,
				Timestamp = msg.Timestamp.ToDateTime(),
				PlatformTimestamp = msg.PlatformTime.ToDateTime(),
				SensorId = ObjectId.Parse(msg.SensorID),
				Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(msg.Longitude, msg.Latitude)),
				Encoding = (MessageEncoding) msg.Encoding
			}).ToList();
		}
	}
}
