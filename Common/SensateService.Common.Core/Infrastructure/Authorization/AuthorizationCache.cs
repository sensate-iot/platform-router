/*
 * Authorization cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Crypto;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Services;

namespace SensateService.Infrastructure.Authorization
{
	public class AuthorizationCache : IAuthorizationCache
	{
		private readonly IServiceProvider m_provider;
		private readonly IDataCache m_cache;
		private SpinLockWrapper m_lock;
		private readonly IList<MeasurementAuthorizationHandler> m_measurementHandler;
		private int m_index;
		private int m_count;

		public AuthorizationCache(IServiceProvider provider, IDataCache cache, IMqttPublishService publisher)
		{
			this.m_provider = provider;
			this.m_cache = cache;
			this.m_index = 0;
			this.m_lock = new SpinLockWrapper();
			this.m_measurementHandler = new List<MeasurementAuthorizationHandler>();

			for(var idx = 0; idx < System.Diagnostics.Process.GetCurrentProcess().Threads.Count; idx++) {
				this.m_measurementHandler.Add( new MeasurementAuthorizationHandler(new SHA256Algorithm(), cache, publisher) );
			}
		}

		public void AddMeasurement(JsonMeasurement data)
		{
			this.m_lock.Lock();

			try {
				var idx = this.GetAndUpdateIndex();

				this.m_count += 1;
				this.m_measurementHandler[idx].AddMessage(data);
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
				this.m_measurementHandler[idx].AddMessages(dataList);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public async Task Load()
		{
			using(var scope = this.m_provider.CreateScope()) {
				var repo = scope.ServiceProvider.GetRequiredService<IAuthorizationRepository>();
				var sensorsTask = repo.GetAllSensorsAsync();
				var userTask = repo.GetAllUsersAsync();
				await Task.WhenAll(sensorsTask, userTask).AwaitBackground();

				Thread.Sleep(500);
				var apiKeys = await repo.GetAllSensorKeysAsync().AwaitBackground();


				this.m_cache.Append(apiKeys);
				this.m_cache.Append(userTask.Result);
				this.m_cache.Append(sensorsTask.Result);
			}
		}

		public int Process()
		{
			Parallel.ForEach(this.m_measurementHandler, async (handler) => {
				await handler.ProcessAsync();
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

			if(this.m_index >= this.m_measurementHandler.Count) {
				this.m_index = 0;
			}

			return idx;
		}
	}
}
