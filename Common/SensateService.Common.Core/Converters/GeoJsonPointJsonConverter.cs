/*
 * JSON serializer for the MongoDB GeoJSON
 * model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using MongoDB.Driver.GeoJsonObjectModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SensateService.Converters
{
	public class GeoJsonPointJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(GeoJsonPoint<GeoJson2DGeographicCoordinates>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			GeoJson2DGeographicCoordinates coords;
			var token = JToken.Load(reader);

			if(reader.TokenType == JsonToken.String && ((string)reader.Value) == String.Empty) {
				coords = new GeoJson2DGeographicCoordinates(0, 0);
			} else {
				var tmp = JsonConvert.DeserializeObject<GeoJsonPointSerializationHelper>(token.ToString());
				coords = new GeoJson2DGeographicCoordinates(tmp.coordinates[0], tmp.coordinates[1]);
			}

			return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(coords);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if(value.GetType().IsArray) {
				writer.WriteStartArray();

				foreach(var item in (Array)value) {
					if(!(item is GeoJsonPoint<GeoJson2DGeographicCoordinates> point))
						return;

					WriteGeoJsonPoint(point, writer);
				}

				writer.WriteEndArray();
			} else {
				if(!(value is GeoJsonPoint<GeoJson2DGeographicCoordinates> point))
					return;

				WriteGeoJsonPoint(point, writer);
			}
		}

		private static void WriteGeoJsonPoint(GeoJsonPoint<GeoJson2DGeographicCoordinates> point, JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("type");
			writer.WriteValue("Point");
			writer.WritePropertyName("coordinates");
			writer.WriteStartArray();
			writer.WriteValue(point.Coordinates.Longitude);
			writer.WriteValue(point.Coordinates.Latitude);
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
	}

	internal class GeoJsonPointSerializationHelper
	{
		public string type { get; set; }
		public IList<double> coordinates { get; set; }
	}
}