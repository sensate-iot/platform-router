/*
 * BSON measurement serialization implementation.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

using SensateService.Models;
using SensateService.Helpers;
using SensateService.Exceptions;
using System.Collections.Generic;

namespace SensateService.Converters
{
	public class BsonMeasurementSerializer : SerializerBase<Measurement>, IBsonDocumentSerializer
	{
		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Measurement value)
		{
			DateTimeOffset offset;
			var writer = context.Writer;

			writer.WriteStartDocument();

			writer.WriteStartDocument("Data");
			this.SerializeDataPoint(writer, value.Data);
			writer.WriteEndDocument();

			if(value.CreatedAt.Kind != DateTimeKind.Utc)
				offset = new DateTimeOffset(value.CreatedAt.ToUniversalTime());
			else
				offset = new DateTimeOffset(value.CreatedAt);

			writer.WriteDateTime("CreatedAt", offset.ToUnixTimeMilliseconds());

			writer.WriteEndDocument();
		}

		private void SerializeDataPoint(IBsonWriter writer, IDictionary<string, DataPoint> points)
		{
			foreach(var dp in points) {
				writer.WriteStartDocument(dp.Key);

				if(!string.IsNullOrEmpty(dp.Value.Unit))
					writer.WriteString("Unit", dp.Value.Unit);

				writer.WriteDecimal128("Value", dp.Value.Value.ToDecimal128());
				writer.WriteEndDocument();
			}
		}

		public void DeserializeAttribute(string attr, IBsonReader reader, ref Measurement m)
		{
			long ticks;

			switch(attr) {
			case "CreatedAt":
				ticks = reader.ReadDateTime();
				m.CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(ticks).UtcDateTime;
				break;

			case "Data":
				this.DeserializeDataPoints(reader, ref m);
				break;

			default:
				throw new DatabaseException("Unknown document attribute", "Measurements");
			}
		}

		private void DeserializeDataPointAttribute(string attr, IBsonReader reader, ref DataPoint dataPoint)
		{
			switch(attr) {
			case "Value":
				dataPoint.Value = reader.ReadDecimal128().ToDecimal();
				break;

			case "Unit":
				if(reader.GetCurrentBsonType() == BsonType.Null) {
					reader.ReadNull();
					dataPoint.Unit = null;
				} else {
					dataPoint.Unit = reader.ReadString();
				}
				break;

			default:
				throw new DatabaseException("Unknown document attribute", "Measurements");
			}
		}

		private void DeserializeDataPoints(IBsonReader reader, ref Measurement m)
		{
			IDictionary<string, DataPoint> datapoints;
			DataPoint dp;
			string key;

			datapoints = new Dictionary<string, DataPoint>();
			reader.ReadStartDocument();

			while(reader.State != BsonReaderState.EndOfDocument) {
				dp = new DataPoint();
				key = reader.ReadName();
				reader.ReadStartDocument();

				while(reader.CurrentBsonType != BsonType.EndOfDocument) {
					this.DeserializeDataPointAttribute(reader.ReadName(), reader, ref dp);
					reader.ReadBsonType();
				}

				reader.ReadEndDocument();
				reader.ReadBsonType();

				datapoints[key] = dp;
			}

			reader.ReadEndDocument();
			m.Data = datapoints;
		}

		public override Measurement Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			Measurement measurement;
			var reader = context.Reader;

			reader.ReadStartDocument();
			measurement = new Measurement();

			while(reader.CurrentBsonType != BsonType.EndOfDocument) {
				this.DeserializeAttribute(reader.ReadName(), reader, ref measurement);
				reader.ReadBsonType();
			}

			reader.ReadEndDocument();
			return measurement;
		}

		public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
		{
			switch(memberName) {
			case "_id":
			case "InternalId":
				serializationInfo = new BsonSerializationInfo(
					memberName, new ObjectIdSerializer(), typeof(ObjectId)
				);
				break;

			case "CreatedAt":
				serializationInfo = new BsonSerializationInfo(
					memberName, new DateTimeSerializer(), typeof(DateTime)
				);
				break;

			default:
				serializationInfo = null;
				return false;
			}

			return true;
		}
	}
}
