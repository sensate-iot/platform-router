/*
 * Information model for the administrative dashboard page.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Auth.Json
{
	public class AdminDashboard
	{
		public int NumberOfUsers { get; set; }
		public int NumberOfSensors { get; set; }
		public int NumberOfActiveUsers { get; set; }
		public int NumberOfGhosts { get; set; }
	}
}