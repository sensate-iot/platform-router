/*
 * Convert messages to and from protobuf format.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Converters
{
	public class MessageProtobufConverter
	{
		public static TextMessageData Convert(IEnumerable<Message> messages)
		{
			var textData = new TextMessageData();

			foreach(var message in messages) {
				var m = new TextMessage {
					Latitude = message.Latitude,
					Longitude = message.Longitude,
					SensorID = message.SensorID.ToString(),
					Timestamp = Timestamp.FromDateTime(message.Timestamp),
					Data = message.Data
				};

				textData.Messages.Add(m);
			}

			return textData;
		}
	}
}