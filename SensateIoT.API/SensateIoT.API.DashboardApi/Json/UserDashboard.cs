/*
 * User dashboard viewmodel.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Newtonsoft.Json.Linq;
using SensateIoT.API.Common.Data.Dto.Json.Out;

namespace SensateIoT.API.DashboardApi.Json
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
				["measurementsToday"] = JToken.FromObject(this.MeasurementsToday.Data) as JArray,
				["measurementsCumulative"] = JToken.FromObject(this.MeasurementsCumulative.Data) as JArray,
				["measurementsPerDayCumulative"] = JToken.FromObject(this.MeasurementsPerDayCumulative.Data) as JArray,
				["apiCallsLastWeek"] = JToken.FromObject(this.ApiCallsLastWeek.Data) as JArray,

				["sensorCount"] = this.SensorCount,
				["measurementsTodayCount"] = this.MeasurementsTodayCount,
				["apiCallCount"] = this.ApiCallCount,
				["securityTokenCount"] = this.SecurityTokenCount
			};

			return json;
		}
	}
}