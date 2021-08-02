/*
 * Converter for Decimal JSON values. Implements the ability to
 * handle null values.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Newtonsoft.Json;

namespace SensateIoT.Platform.Router.Data.Converters
{
	public class DecimalJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(decimal) || objectType == typeof(decimal?);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if(reader.TokenType == JsonToken.String && ((string)reader.Value) == string.Empty)
				return decimal.MinValue;

			if(reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
				return Convert.ToDecimal(reader.Value);

			throw new JsonSerializationException(
				$"Unexpected token type: {reader.TokenType.ToString()}"
			);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = (decimal)value;

			if(v == decimal.MinValue) {
				writer.WriteValue(string.Empty);
			} else {
				writer.WriteValue(value);
			}
		}
	}
}
