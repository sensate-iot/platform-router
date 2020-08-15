/*
 * user service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Services.Processing
{
	public class UserService : IUserService
	{
		private readonly ISensorStatisticsRepository m_stats;
		private readonly ISensorService m_sensors;
		private readonly ISensorRepository m_sensorDb;
		private readonly UserManager<SensateUser> m_users;
		private readonly IAuditLogRepository m_logs;

		public UserService(
			ISensorService sensors,
			UserManager<SensateUser> users,
			IAuditLogRepository logs,
			ISensorRepository sensorDb,
			ISensorStatisticsRepository stats
		)
		{
			this.m_users = users;
			this.m_logs = logs;
			this.m_sensors = sensors;
			this.m_stats = stats;
			this.m_sensorDb = sensorDb;
		}

		public async Task DeleteAsync(SensateUser user, CancellationToken ct = default)
		{
			await this.m_users.DeleteAsync(user).AwaitBackground();
			var @enum = await this.m_sensorDb.GetAsync(user).AwaitBackground();

			var sensors = @enum.ToList();
			var sensorTasks = new Task[sensors.Count];
			var statsTasks = new Task[sensors.Count];

			for(var idx = 0; idx < sensors.Count; idx++) {
				sensorTasks[idx] = this.m_sensors.DeleteAsync(sensors[idx], ct);
				statsTasks[idx] = this.m_stats.DeleteBySensorAsync(sensors[idx], ct);

			}

			await Task.WhenAll(sensorTasks).AwaitBackground();
			await Task.WhenAll(statsTasks).AwaitBackground();
			await this.m_logs.DeleteBetweenAsync(user, DateTime.MinValue, DateTime.Now);
		}
	}
}