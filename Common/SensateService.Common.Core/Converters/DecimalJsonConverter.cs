/*
 * Converter for Decimal JSON values. Implements the ability to
 * handle null values.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace SensateService.Converters
{
	public class DecimalJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(decimal) || objectType == typeof(decimal?);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if(reader.TokenType == JsonToken.String && ((string)reader.Value) == String.Empty)
				return Decimal.MinValue;
			else if(reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
				return Convert.ToDecimal(reader.Value);

			throw new JsonSerializationException(
				String.Format("Unexpected token type: {0}", reader.TokenType.ToString())
			);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			decimal v = (decimal)value;
			string raw;

			if(v == Decimal.MinValue || value == null) {
				writer.WriteValue(String.Empty);
			} else {
				raw = v.ToString(CultureInfo.InvariantCulture);
				writer.WriteValue(value);
			}
		}
	}
}
