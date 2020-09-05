/*
 * Authorization cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Generic;
using SensateService.Common.Data.Enums;
using SensateService.Crypto;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure.Authorization
{
	public class AuthorizationCache : IAuthorizationCache
	{
		private readonly IServiceProvider m_provider;
		private readonly IDataCache m_cache;
		private readonly IList<MeasurementAuthorizationHandler> m_measurementsHandler;
		private readonly IList<MessageAuthorizationHandler> m_messageHandlers;

		private SpinLockWrapper m_lock;
		private int m_index;
		private int m_count;

		private SpinLockWrapper m_commandLock;
		private IList<Command> m_commands;

		public static event OnDataAuthorizedEvent MeasurementDataAuthorized;
		public static event OnDataAuthorizedEvent MessageDataAuthorized;

		public AuthorizationCache(IServiceProvider provider, IDataCache cache)
		{
			this.m_provider = provider;
			this.m_cache = cache;
			this.m_index = 0;
			this.m_lock = new SpinLockWrapper();
			this.m_commandLock = new SpinLockWrapper();
			this.m_commands = new List<Command>();
			this.m_measurementsHandler = new List<MeasurementAuthorizationHandler>();
			this.m_messageHandlers = new List<MessageAuthorizationHandler>();

			var factory = new LoggerFactory();

			for(var idx = 0; idx < System.Diagnostics.Process.GetCurrentProcess().Threads.Count; idx++) {
				this.m_measurementsHandler.Add(new MeasurementAuthorizationHandler(new SHA256Algorithm(), cache, factory.CreateLogger<MeasurementAuthorizationHandler>()));
				this.m_messageHandlers.Add(new MessageAuthorizationHandler(new SHA256Algorithm(), cache, factory.CreateLogger<MessageAuthorizationHandler>()));
			}
		}

		public void AddMeasurement(JsonMeasurement data)
		{
			this.m_lock.Lock();

			try {
				var idx = this.GetAndUpdateIndex();

				this.m_count += 1;
				this.m_measurementsHandler[idx].AddMessage(data);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public void AddMeasurements(IEnumerable<JsonMeasurement> data)
		{
			this.m_lock.Lock();

			try {
				var idx = this.GetAndUpdateIndex();
				var dataList = data.ToList();

				this.m_count += dataList.Count();
				this.m_measurementsHandler[idx].AddMessages(dataList);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public void AddMessage(JsonMessage data)
		{
			this.m_lock.Lock();

			try {
				var idx = this.GetAndUpdateIndex();

				this.m_count += 1;
				this.m_messageHandlers[idx].AddMessage(data);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public void AddMessages(IEnumerable<JsonMessage> data)
		{
			this.m_lock.Lock();

			try {
				var idx = this.GetAndUpdateIndex();
				var dataList = data.ToList();

				this.m_count += dataList.Count();
				this.m_messageHandlers[idx].AddMessages(dataList);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public async Task Load()
		{
			await this.m_cache.Clear().AwaitBackground();

			using var scope = this.m_provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<IAuthorizationRepository>();
			var sensorsTask = repo.GetAllSensorsAsync();
			var userTask = repo.GetAllUsersAsync();
			await Task.WhenAll(sensorsTask, userTask).AwaitBackground();

			var apiKeys = await repo.GetAllSensorKeysAsync().AwaitBackground();

			this.m_cache.Append(apiKeys);
			this.m_cache.Append(userTask.Result);
			this.m_cache.Append(sensorsTask.Result);

			CollectAfterLargeReload();
		}

		public static void CollectAfterLargeReload()
		{
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect();
		}

		public int Process()
		{
			Parallel.ForEach(this.m_measurementsHandler, async handler => {
				await handler.ProcessAsync().AwaitBackground();
			});

			Parallel.ForEach(this.m_messageHandlers, async handler => {
				await handler.ProcessAsync().AwaitBackground();
			});

			int rv;

			this.m_lock.Lock();
			rv = this.m_count;
			this.m_count = 0;
			this.m_lock.Unlock();

			return rv;
		}

		public void AddCommand(Command cmd)
		{
			this.m_commandLock.Lock();

			try {
				this.m_commands.Add(cmd);
			} finally {
				this.m_commandLock.Unlock();
			}
		}

		public static Task InvokeMessageEvent(object measurementAuthorizationHandler, DataAuthorizedEventArgs args)
		{
			return MessageDataAuthorized != null ? MessageDataAuthorized.Invoke(measurementAuthorizationHandler, args) : Task.CompletedTask;
		}

		public static Task InvokeMeasurementEvent(object measurementAuthorizationHandler, DataAuthorizedEventArgs args)
		{
			return MeasurementDataAuthorized != null ? MeasurementDataAuthorized.Invoke(measurementAuthorizationHandler, args) : Task.CompletedTask;
		}

		public async Task ProcessCommandsAsync()
		{
			IList<Command> cmds = new List<Command>();
			using var scope = this.m_provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<IAuthorizationRepository>();

			this.m_commandLock.Lock();

			try {
				var tmp = this.m_commands;
				this.m_commands = cmds;
				cmds = tmp;
			} finally {
				this.m_commandLock.Unlock();
			}

			if(cmds.Count <= 0) {
				return;
			}

			foreach(var command in cmds) {
				switch(command.Cmd) {
				case AuthServiceCommand.FlushUser:
					this.m_cache.RemoveUser(command.Arguments);
					break;

				case AuthServiceCommand.FlushSensor:
					if(ObjectId.TryParse(command.Arguments, out var id)) {
						this.m_cache.RemoveSensor(id);
					}
					break;

				case AuthServiceCommand.FlushKey:
					this.m_cache.RemoveKey(command.Arguments);
					break;

				case AuthServiceCommand.AddUser:
					var user = await repo.GetUserAsync(command.Arguments).AwaitBackground();
					break;

				case AuthServiceCommand.AddSensor:
					break;

				case AuthServiceCommand.AddKey:
					var key = await repo.GetSensorKeyAsync(command.Arguments).AwaitBackground();
					break;

				default:
					continue;
				}
			}
		}

		private int GetAndUpdateIndex()
		{
			var idx = this.m_index;

			this.m_index += 1;

			if(this.m_index >= this.m_measurementsHandler.Count) {
				this.m_index = 0;
			}

			return idx;
		}
	}
}
