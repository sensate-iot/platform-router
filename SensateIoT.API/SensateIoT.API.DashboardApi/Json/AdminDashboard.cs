/*
 * Information model for the administrative dashboard page.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Newtonsoft.Json.Linq;
using SensateIoT.API.Common.Data.Dto.Json.Out;

namespace SensateIoT.API.DashboardApi.Json
{
	public class AdminDashboard
	{
		public int NumberOfUsers { get; set; }
		public int NumberOfGhosts { get; set; }
		public long NumberOfSensors { get; set; }
		public long MeasurementStatsLastHour { get; set; }

		public Graph<DateTime, int> Registrations { get; set; }
		public Graph<DateTime, long> MeasurementStats { get; set; }

		public JObject ToJson()
		{
			JObject json = new JObject {
				["numberOfUsers"] = this.NumberOfUsers,
				["numberOfSensors"] = this.NumberOfSensors,
				["measurementStatsLastHour"] = this.MeasurementStatsLastHour,
				["numberOfGhosts"] = this.NumberOfGhosts,
				["registrations"] = JToken.FromObject(this.Registrations.Data) as JArray,
				["measurementStats"] = JToken.FromObject(this.MeasurementStats.Data) as JArray
			};

			return json;
		}
	}
}
