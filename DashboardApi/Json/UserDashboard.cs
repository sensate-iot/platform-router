/*
 * User dashboard viewmodel.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Newtonsoft.Json.Linq;
using SensateService.Models.Json.Out;

namespace SensateService.DashboardApi.Json
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
				["measurementsToday"] = JToken.FromObject(MeasurementsToday.Data) as JArray,
				["measurementsCumulative"] = JToken.FromObject(MeasurementsCumulative.Data) as JArray,
				["measurementsPerDayCumulative"] = JToken.FromObject(MeasurementsPerDayCumulative.Data) as JArray,
				["apiCallsLastWeek"] = JToken.FromObject(ApiCallsLastWeek.Data) as JArray,

				["sensorCount"] = SensorCount,
				["measurementsTodayCount"] = MeasurementsTodayCount,
				["apiCallCount"] = ApiCallCount,
				["securityTokenCount"] = SecurityTokenCount
			};

			return json;
		}
	}
}