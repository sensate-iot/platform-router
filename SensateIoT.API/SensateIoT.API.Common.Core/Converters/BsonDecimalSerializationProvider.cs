/*
 * Binary conerter provider for BSON documents.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SensateIoT.API.Common.Core.Converters
{
	public class BsonDecimalSerializationProvider : IBsonSerializationProvider
	{
		private static DecimalSerializer DecimalSerializer = new DecimalSerializer(BsonType.Decimal128);
		private static NullableSerializer<Decimal> NullableSerializer = new NullableSerializer<decimal>(
			new DecimalSerializer(BsonType.Decimal128)
		);

		public IBsonSerializer GetSerializer(Type type)
		{
			if(type == typeof(decimal))
				return DecimalSerializer;

			if(type == typeof(decimal?))
				return NullableSerializer;

			return null;
		}
	}
}
