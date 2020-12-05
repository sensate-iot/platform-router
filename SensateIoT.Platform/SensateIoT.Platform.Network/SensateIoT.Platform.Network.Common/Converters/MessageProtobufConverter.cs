/*
 * Convert messages to and from protobuf format.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf.WellKnownTypes;

using MongoDB.Bson;

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

		public static IEnumerable<Message> Convert(TextMessageData messages)
		{
			return messages.Messages.Select(Convert);
		}

		public static TextMessage Convert(Message message)
		{
			return new TextMessage {
				Latitude = message.Latitude,
				Longitude = message.Longitude,
				SensorID = message.SensorID.ToString(),
				Timestamp = Timestamp.FromDateTime(message.Timestamp),
				PlatformTime = Timestamp.FromDateTime(message.PlatformTimestamp),
				Data = message.Data
			};
		}

		public static Message Convert(TextMessage message)
		{
			return new Message {
				Timestamp = message.Timestamp == null ? DateTime.UtcNow : message.Timestamp.ToDateTime(),
				PlatformTimestamp = message.PlatformTime == null ? DateTime.UtcNow : message.PlatformTime.ToDateTime(),
				Longitude = message.Longitude,
				Latitude = message.Latitude,
				Data = message.Data,
				SensorId = ObjectId.Parse(message.SensorID)
			};
		}
	}
}
