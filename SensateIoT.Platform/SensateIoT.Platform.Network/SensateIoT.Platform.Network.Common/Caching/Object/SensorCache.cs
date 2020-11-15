/*
 * Type safe in-memory sensor cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public sealed class SensorCache : MemoryCache<ObjectId, Sensor>, ISensorCache
	{
		private HashSet<ObjectId> m_liveSensors;
		private readonly HashSet<LiveDataRoute> m_liveDataEntries;

		private bool m_disposed;

		public SensorCache(long capacity, TimeSpan tmo) : base(capacity, tmo)
		{
			this.m_liveSensors = new HashSet<ObjectId>();
			this.m_liveDataEntries = new HashSet<LiveDataRoute>(new LiveDataRouteEqualityComparer());
			this.m_disposed = false;
		}

		public override void AddOrUpdate(IEnumerable<Abstract.KeyValuePair<ObjectId, Sensor>> values, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheEntryOptions(options);
			// ReSharper disable once PossibleMultipleEnumeration
			ValidateKeyValuePairs(values);

			this.m_dataLock.EnterWriteLock();

			try {
				// ReSharper disable once PossibleMultipleEnumeration
				foreach(var pair in values) {
					if(this.m_liveSensors.Contains(pair.Key)) {
						var sensor = this.m_data[pair.Key];
						pair.Value.LiveDataRouting = sensor.Value.LiveDataRouting;
					}

					this.ValidateCacheKey(pair.Key, false);
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public override void AddOrUpdate(ObjectId key, Sensor value, CacheEntryOptions options = null)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				var result = this.m_data.TryGetValue(key, out var sensor);

				if(result) {
					value.LiveDataRouting = sensor.Value.LiveDataRouting;
				}

				base.AddOrUpdate(key, value, options);
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void AddLiveDataRouting(LiveDataRoute route)
		{
			this.m_dataLock.EnterWriteLock();

			try {
                this.m_liveSensors.Add(route.SensorId);
				this.m_liveDataEntries.Add(route);

                var sensor = this.m_data[route.SensorId];

                sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>(new RoutingTargetEqualityComparer());
                sensor.Value.LiveDataRouting.Add(new RoutingTarget { Target = route.Target, Type = RouteType.LiveDataSubscription});
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void RemoveLiveDataRouting(LiveDataRoute route)
		{
			this.m_dataLock.EnterWriteLock();

			try {
                var sensor = this.m_data[route.SensorId];
                sensor.Value.LiveDataRouting?.Remove(new RoutingTarget {Target = route.Target, Type = RouteType.LiveDataSubscription});

                if(sensor.Value.LiveDataRouting?.Count <= 0) {
	                this.m_liveSensors.Remove(route.SensorId);
                }
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void SyncLiveData(string origin, ICollection<LiveDataRoute> data)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				foreach(var entry in data) {
					this.m_liveDataEntries.Add(entry);
					this.m_liveSensors.Add(entry.SensorId);
				}

			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void FlushLiveData()
		{
			this.m_dataLock.EnterWriteLock();

			try {
				this.m_liveSensors = new HashSet<ObjectId>();

				foreach(var entry in this.m_liveDataEntries) {
					var sensor = this.m_data[entry.SensorId].Value;

					if(!this.m_liveSensors.Contains(entry.SensorId)) {
						var liveData = new HashSet<RoutingTarget>(new RoutingTargetEqualityComparer());

						liveData.UnionWith(sensor.LiveDataRouting?.Where(x => x.Type != RouteType.LiveDataSubscription) ?? Array.Empty<RoutingTarget>());
						sensor.LiveDataRouting = liveData;
						this.m_liveSensors.Add(entry.SensorId);
					}

					sensor.LiveDataRouting.Add(new RoutingTarget {Target = entry.Target, Type = RouteType.LiveDataSubscription});
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		[AssertionMethod]
		private void CheckDisposed()
		{
			if(this.m_disposed) {
				throw new ObjectDisposedException(nameof(SensorCache));
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			this.m_disposed = true;
		}
	}
}
