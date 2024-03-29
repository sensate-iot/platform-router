﻿/*
 * Update data in real-time.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Data.DTO;
using SensateIoT.Platform.Router.Data.Enums;
using SensateIoT.Platform.Router.DataAccess.Abstract;

namespace SensateIoT.Platform.Router.Common.Caching.Routing
{
	public class DataUpdateHandler
	{
		private readonly IRoutingCache m_cache;
		private readonly IServiceProvider m_provider;

		public DataUpdateHandler(IRoutingCache cache, IServiceProvider provider)
		{
			this.m_cache = cache;
			this.m_provider = provider;
		}

		public async Task UpdateAsync(Command cmd, CancellationToken ct)
		{
			switch(cmd.Cmd) {
			case CommandType.DeleteUser:
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
					var userRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();
					var user = await userRepo.GetAccountForRoutingAsync(userGuid, ct).ConfigureAwait(false);

					this.m_cache.Append(user);
				}
				break;

			case CommandType.AddSensor:
				var sensorId = ObjectId.Parse(cmd.Arguments);
				await this.ReloadSensor(sensorId, ct).ConfigureAwait(false);
				break;

			case CommandType.AddKey:
				using(var scope = this.m_provider.CreateScope()) {
					var userRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();
					var key = await userRepo.GetApiKeyAsync(cmd.Arguments, ct).ConfigureAwait(false);

					this.m_cache.Append(cmd.Arguments, key);
				}
				break;

			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private async Task ReloadSensor(ObjectId sensorId, CancellationToken ct)
		{
			using var scope = this.m_provider.CreateScope();
			var routingRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();

			var triggers = routingRepo.GetTriggerInfoAsync(sensorId, default);
			var sensor = await routingRepo.GetSensorsByIDAsync(sensorId, ct).ConfigureAwait(false);
			await triggers.ConfigureAwait(false);

			sensor.TriggerInformation = triggers.Result.Select(x => new SensorTrigger {
				HasActions = x.ActionCount > 0,
				IsTextTrigger = x.TextTrigger
			}).ToList();

			this.m_cache[sensorId] = sensor;
		}
	}
}
