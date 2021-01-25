using System.Collections.Generic;
using System.Linq;
using SensateIoT.API.Common.Data.Dto.Generic;

namespace SensateIoT.API.Common.Data.Converters
{
	public class MessageConverter
	{
		public static Message Convert(Models.Message message)
		{
			return new Message {
				Data = message.Data,
				Encoding = message.Encoding,
				InternalId = message.InternalId,
				Location = new GeoJsonPoint {
					Latitude = message.Location.Coordinates.Latitude,
					Longitude = message.Location.Coordinates.Longitude
				},
				PlatformTimestamp = message.PlatformTimestamp,
				SensorId = message.SensorId,
				Timestamp = message.Timestamp
			};
		}

		public static IEnumerable<Message> Convert(IEnumerable<Models.Message> messages)
		{
			return messages.Select(Convert).ToList();
		}
	}
}