/*
 * Measurement filter.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using SensateService.Enums;

namespace SensateService.DataApi.Models
{
	public class Filter
	{
		public IList<string> SensorIds { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int? Skip { get; set; }
		public int? Limit { get; set; }
		public double? Longitude { get; set; }
		public double? Latitude { get; set; }
		public int? Radius { get; set; }
		public string OrderDirection { get; set; }
	}
}