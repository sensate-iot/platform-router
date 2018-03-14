/*
 * Converter for BsonDocuments (MongoDB).
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq;
using System.Collections.Generic;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SensateService.Converters
{
	public class BsonDocumentConverter : JsonConverter
	{
		public static readonly JsonWriterSettings JsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };

		public override bool CanConvert(Type objectType)
		{
			return typeof(BsonDocument) == objectType;
		}

		public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			List<BsonDocument> docs;
			var token = JToken.Load(reader);
			BsonDocument document;


			if(token.Type == JTokenType.Array) {
				docs = new List<BsonDocument>();
				foreach(var docstr in token.ToObject<string[]>()) {
					if(!BsonDocument.TryParse(docstr, out document))
						continue;

					docs.Add(document);
				}

				return docs.AsEnumerable();
			}

			if(!BsonDocument.TryParse(token.ToString(), out document))
				return null;

			return document;
		}

		public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer)
		{
			JToken token;
			BsonDocument document;

			if(value.GetType().IsArray) {
				writer.WriteStartArray();

				foreach(var item in (Array)value) {
					document = item as BsonDocument;
					var tmp = document.ToJson(BsonDocumentConverter.JsonWriterSettings);
					token = JToken.Parse(tmp);
					serializer.Serialize(writer, token);
				}

				writer.WriteEndArray();
			} else {
				document = value as BsonDocument;
				var tmp = document.ToJson(BsonDocumentConverter.JsonWriterSettings);
				token = JToken.Parse(tmp);
				serializer.Serialize(writer, token);
			}
		}
	}
}
