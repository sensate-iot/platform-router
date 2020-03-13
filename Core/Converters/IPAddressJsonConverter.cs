/*
 * IPAddress converter for Newtonsoft JSON.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Net;

using Newtonsoft.Json;

namespace SensateService.Converters
{
	public class IPAddressJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(IPAddress));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
		                                JsonSerializer serializer)
		{
			return IPAddress.Parse((string) reader.Value);
		}
	}
}