/*
 * Protobuf control message converter.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Converters
{
	public class ControlMessageProtobufConverter
	{
		public static ControlMessage Convert(Contracts.DTO.ControlMessage message)
		{
			return new ControlMessage {
				Timestamp = message.Timestamp?.ToDateTime() ?? DateTime.UtcNow,
				Data = message.Data,
				Secret = "",
				SensorId = ObjectId.Parse(message.SensorID)
			};
		}

		public static IEnumerable<ControlMessage> Convert(Contracts.DTO.ControlMessageData message)
		{
			return message.Messages.Select(Convert);
		}
	}
}
