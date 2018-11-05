/*
 * Information model for the administrative dashboard page.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using Newtonsoft.Json.Linq;
using SensateService.Models.Json.Out;

namespace SensateService.Auth.Json
{
	public class AdminDashboard
	{
		public int NumberOfUsers { get; set; }
		public int NumberOfSensors { get; set; }
		public int NumberOfActiveUsers { get; set; }
		public int NumberOfGhosts { get; set; }

		public Graph<DateTime, int> Registrations { get; set; }

		public JObject ToJson()
		{
			JObject json = new JObject {
				["numberOfUsers"] = NumberOfUsers,
				["numberOfSensors"] = NumberOfSensors,
				["numberOfActiveUsers"] = NumberOfActiveUsers,
				["numberOfGhosts"] = NumberOfActiveUsers,
				["registrations"] = JToken.FromObject(Registrations.Data) as JArray
			};


			return json;
		}
	}
}