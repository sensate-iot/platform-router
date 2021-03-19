using System;
using Google.Protobuf;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Converters
{
	public static class TriggerActionEventConverter
	{
		public static TriggerEvent Convert(TriggerAction action)
		{
			return new TriggerEvent {
				SensorID = ByteString.CopyFrom(action.SensorID.ToByteArray()),
				Type = ConvertChannel(action.Channel)
			};
		}

		private static TriggerEventType ConvertChannel(TriggerChannel channel)
		{
			switch(channel) {
			case TriggerChannel.Email:
				return TriggerEventType.Email;

			case TriggerChannel.SMS:
				return TriggerEventType.Sms;

			case TriggerChannel.LiveData:
				return TriggerEventType.LiveData;

			case TriggerChannel.MQTT:
				return TriggerEventType.Mqtt;

			case TriggerChannel.HttpPost:
				return TriggerEventType.HttpPost;

			case TriggerChannel.HttpGet:
				return TriggerEventType.HttpGet;

			case TriggerChannel.ControlMessage:
				return TriggerEventType.ControlMessage;

			default:
				throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
			}
		}
	}
}