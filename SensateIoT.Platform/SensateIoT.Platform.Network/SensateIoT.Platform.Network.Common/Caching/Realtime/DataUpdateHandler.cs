/*
 * Update data in real-time.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.DataAccess.Repositories;

namespace SensateIoT.Platform.Network.Common.Caching.Realtime
{
	public class DataUpdateHandler
	{
		private readonly IDataCache m_cache;
		private readonly IServiceProvider m_provider;

		public DataUpdateHandler(IDataCache cache, IServiceProvider provider)
		{
			this.m_cache = cache;
			this.m_provider = provider;
		}

		public async Task UpdateAsync(Command cmd, CancellationToken ct)
		{
			switch(cmd.Cmd) {
			case CommandType.FlushUser:
				var userId = Guid.Parse(cmd.Arguments);
				this.m_cache.RemoveAccount(userId);
				break;

			case CommandType.FlushSensor:
				var objId = ObjectId.Parse(cmd.Arguments);
				this.m_cache.RemoveSensor(objId);
				break;

			case CommandType.FlushKey:
				this.m_cache.RemoveApiKey(cmd.Arguments);
				break;

			case CommandType.AddUser:
				var userGuid = Guid.Parse(cmd.Arguments);

				using(var scope = this.m_provider.CreateScope()) {
					var userRepo = scope.ServiceProvider.GetRequiredService<IAccountsRepository>();
					var user = await userRepo.GetAccountAsync(userGuid, ct).ConfigureAwait(false);

					this.m_cache.Append(user);
				}
				break;

			case CommandType.AddSensor:
				var sensorId = ObjectId.Parse(cmd.Arguments);
				await this.ReloadSensor(sensorId, ct).ConfigureAwait(false);
				break;

			case CommandType.AddKey:
				using(var scope = this.m_provider.CreateScope()) {
					var userRepo = scope.ServiceProvider.GetRequiredService<IAccountsRepository>();
					var key = await userRepo.GetApiKeyAsync(cmd.Arguments, ct).ConfigureAwait(false);

					this.m_cache.Append(key);
				}
				break;

			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private async Task ReloadSensor(ObjectId sensorId, CancellationToken ct)
		{
			using var scope = this.m_provider.CreateScope();
			var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
			var triggerRepo = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

			var triggers = triggerRepo.GetTriggerInfoAsync(sensorId, default);
			var sensor = await sensorRepo.GetSensorsByIDAsnc(sensorId, ct).ConfigureAwait(false);
			await triggers.ConfigureAwait(false);

			sensor.TriggerInformation = new SensorTrigger {
				HasActions = triggers.Result.ActionCount > 0,
				IsTextTrigger = triggers.Result.TextTrigger
			};

			this.m_cache.Append(sensor);
		}
	}
}
