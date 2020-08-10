/*
 * Trigger (text) message.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using MongoDB.Driver.GeoJsonObjectModel;

namespace SensateService.TriggerHandler.Models
{
	public class Message
	{
		public DateTime Timestamp { get; set; }
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
		public string Data { get; set; }
	}
}