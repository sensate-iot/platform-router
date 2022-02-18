/*
 * Validate messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Validators
{
	public static class MessageValidator
	{
		private const int MaxLat = 90;
		private const int MinLat = -90;
		private const int MaxLon = 180;
		private const int MinLon = -180;

		public static bool Validate(Message message)
		{
			if(EpsilonCompare(message.Latitude, 0D) && EpsilonCompare(message.Longitude, 0D)) {
				return true;
			}

			return message.Latitude >= MinLat && message.Latitude <= MaxLat && message.Longitude >= MinLon && message.Longitude <= MaxLon;
		}

		private static bool EpsilonCompare(double double1, double double2)
		{
			return (Math.Abs(double1 - double2) <= double.Epsilon);
		}
	}
}
