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
using SensateService.Common.Data.Dto.Authorization;
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

		public static event OnDataAuthorizedEvent MeasurementDataAuthorized;
		public static event OnDataAuthorizedEvent MessageDataAuthorized;

		public AuthorizationCache(IServiceProvider provider, IDataCache cache)
		{
			this.m_provider = provider;
			this.m_cache = cache;
			this.m_index = 0;
			this.m_lock = new SpinLockWrapper();
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

		private int GetAndUpdateIndex()
		{
			var idx = this.m_index;

			this.m_index += 1;

			if(this.m_index >= this.m_measurementsHandler.Count) {
				this.m_index = 0;
			}

			return idx;
		}

		public static Task InvokeMessageEvent(object measurementAuthorizationHandler, DataAuthorizedEventArgs args)
		{
			if(MessageDataAuthorized != null) {
				return MessageDataAuthorized.Invoke(measurementAuthorizationHandler, args);
			}

			return Task.CompletedTask;
		}

		public static Task InvokeMeasurementEvent(object measurementAuthorizationHandler, DataAuthorizedEventArgs args)
		{
			if(MeasurementDataAuthorized != null) {
				return MeasurementDataAuthorized.Invoke(measurementAuthorizationHandler, args);
			}

			return Task.CompletedTask;
		}
	}
}
