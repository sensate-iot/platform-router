/*
 * Model representing minimal measurement information.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace SensateService.Models.Json.In
{
	public class RawMeasurement
	{
		public JContainer Data { private get; set; }
		public double? Longitude { get; set; }
		public double? Latitude { get; set; }
		public Nullable<DateTime> CreatedAt { get; set; }
		public string CreatedBySecret { get; set; }
		public string CreatedById { get; set; }

		public bool IsCreatedBy(Sensor sensor) => this.CreatedBySecret == sensor.Secret;

		public bool TryParseData(out IDictionary<string, DataPoint> datapoints) =>
			Measurement.TryParseData(this.Data, out datapoints);
	}
}
