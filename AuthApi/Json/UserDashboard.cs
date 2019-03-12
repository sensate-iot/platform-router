/*
 * User dashboard viewmodel.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

using Newtonsoft.Json.Linq;
using SensateService.Models.Json.Out;

namespace SensateService.Auth.Json
{
	public class UserDashboard
	{
		public long SensorCount { get; set; }
		public long MeasurementsTodayCount { get; set; }
		public long ApiCallCount { get; set; }
		public long SecurityTokenCount { get; set; }

		public Graph<DateTime, long> MeasurementsToday { get; set; }
		public Graph<DateTime, long> MeasurementsCumulative { get; set; }
		public Graph<DateTime, long> ApiCallsLastWeek { get; set; }
		public Graph<int, long> MeasurementsPerDayCumulative { get; set; }

		public JObject ToJson()
		{
			JObject json = new JObject {
				["MeasurementsToday"] = JToken.FromObject(MeasurementsToday.Data) as JArray,
				["MeasurementsCumulative"] = JToken.FromObject(MeasurementsCumulative.Data) as JArray,
				["MeasurementsPerDayCumulative"] = JToken.FromObject(MeasurementsPerDayCumulative.Data) as JArray,
				["ApiCallsLastWeek"] = JToken.FromObject(ApiCallsLastWeek.Data) as JArray,

				["SensorCount"] = SensorCount,
				["MeasurementsTodayCount"] = MeasurementsTodayCount,
				["ApiCallCount"] = ApiCallCount,
				["SecurityTokenCount"] = SecurityTokenCount
			};

			return json;
		}
	}
}