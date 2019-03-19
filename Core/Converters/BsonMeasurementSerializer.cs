/*
 * BSON measurement serialization implementation.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq;

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
			writer.WriteObjectId("_id", value.InternalId);

			writer.WriteName("Data");
			writer.WriteStartArray();
			value.Data.ToList().ForEach(x => this.SerializeDataPoint(writer, x));
			writer.WriteEndArray();

			writer.WriteDouble("Longitude", value.Longitude);
			writer.WriteDouble("Latitude", value.Latitude);

			if(value.CreatedAt.Kind != DateTimeKind.Utc)
				offset = new DateTimeOffset(value.CreatedAt.ToUniversalTime());
			else
				offset = new DateTimeOffset(value.CreatedAt);

			writer.WriteDateTime("CreatedAt", offset.ToUnixTimeMilliseconds());
			writer.WriteObjectId("CreatedBy", value.CreatedBy);

			writer.WriteEndDocument();
		}

		private void SerializeDataPoint(IBsonWriter writer, DataPoint dp)
		{
			writer.WriteStartDocument();

			writer.WriteString("Name", dp.Name);
			writer.WriteDecimal128("Value", dp.Value.ToDecimal128());

			if(dp.Unit == null)
				writer.WriteNull("Unit");
			else
				writer.WriteString("Unit", dp.Unit);

			writer.WriteEndDocument();
		}

		public void DeserializeAttribute(string attr, IBsonReader reader, ref Measurement m)
		{
			long ticks;

			switch(attr) {
			case "_id":
				m.InternalId = reader.ReadObjectId();
				break;

			case "CreatedBy":
				m.CreatedBy = reader.ReadObjectId();
				break;

			case "CreatedAt":
				ticks = reader.ReadDateTime();
				m.CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(ticks).UtcDateTime;
				break;

			case "Longitude":
				m.Longitude = reader.ReadDouble();
				break;

			case "Latitude":
				m.Latitude = reader.ReadDouble();
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
			case "Name":
				dataPoint.Name = reader.ReadString();
				break;

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
			DataPoint dp;
			List<DataPoint> datapoints;

			datapoints = new List<DataPoint>();
			reader.ReadStartArray();

			while(reader.State != BsonReaderState.EndOfArray) {
				dp = new DataPoint();
				reader.ReadStartDocument();

				while(reader.CurrentBsonType != BsonType.EndOfDocument) {
					this.DeserializeDataPointAttribute(reader.ReadName(), reader, ref dp);
					reader.ReadBsonType();
				}

				datapoints.Add(dp);
				reader.ReadEndDocument();
				reader.ReadBsonType();
			}
			reader.ReadEndArray();

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
			case "CreatedBy":
				serializationInfo = new BsonSerializationInfo(
					memberName, new ObjectIdSerializer(), typeof(ObjectId)
				);
				break;

			case "CreatedAt":
				serializationInfo = new BsonSerializationInfo(
					memberName, new DateTimeSerializer(), typeof(DateTime)
				);
				break;

			case "Longitude":
			case "Latitude":
				serializationInfo = new BsonSerializationInfo(
					memberName, new DoubleSerializer(), typeof(double)
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
