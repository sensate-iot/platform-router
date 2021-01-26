using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;

namespace SensateIoT.API.Common.Data.Dto.Generic
{
	[BsonIgnoreExtraElements]
	public class GeoJsonPoint
	{
		[BsonIgnore]
		public string Type => "Point";
		public double[] Coordinates { get; }

		public GeoJsonPoint()
		{
			this.Coordinates = new double[2];
		}

		[JsonIgnore]
		public double Longitude {
			get => this.Coordinates[0];
			set => this.Coordinates[0] = value;
		}

		[JsonIgnore]
		public double Latitude {
			get => this.Coordinates[1];
			set => this.Coordinates[1] = value;
		}

		public GeoJson2DGeographicCoordinates ToCoordinates()
		{
			return new GeoJson2DGeographicCoordinates(this.Longitude, this.Latitude);
		}
	}
}
