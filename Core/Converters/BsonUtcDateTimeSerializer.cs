/*
 * BSON serializer to force UTC date time objects in MongoDB.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SensateService.Converters
{
	public class BsonUtcDateTimeSerializer : DateTimeSerializer
	{
		public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var obj = base.Deserialize(context, args);

			if(obj.Kind != DateTimeKind.Utc)
				obj = new DateTime(obj.Ticks, DateTimeKind.Utc);

			return obj;
		}

		public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, DateTime value)
		{
			if(value.Kind != DateTimeKind.Utc)
				value = value.ToUniversalTime();

			base.Serialize(ctx, args, value);
		}
	}
}