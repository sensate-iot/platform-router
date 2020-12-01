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
using SensateIoT.Platform.Network.Common.Caching.Comparators;
using SensateIoT.Platform.Network.Common.Caching.Memory;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public sealed class SensorCache : MemoryCache<ObjectId, Sensor>, ISensorCache
	{
		private HashSet<ObjectId> m_liveSensors;
		private HashSet<LiveDataRoute> m_liveDataEntries;
		private HashSet<LiveDataRoute> m_liveDataSyncEntries;

		private bool m_disposed;

		public SensorCache(long capacity, TimeSpan tmo) : base(capacity, tmo)
		{
			this.m_liveSensors = new HashSet<ObjectId>();
			this.m_liveDataEntries = new HashSet<LiveDataRoute>(new LiveDataRouteEqualityComparer());
			this.m_liveDataSyncEntries = new HashSet<LiveDataRoute>(new LiveDataRouteEqualityComparer());
			this.m_disposed = false;
		}

		public override void Add(ObjectId key, Sensor value, CacheEntryOptions options = null)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				this.ValidateCacheKey(key, true);
				base.AddOrUpdate(key, value, options);

				foreach(var entry in this.m_liveDataEntries) {
					if(entry.SensorID != key || !this.m_data.TryGetValue(entry.SensorID, out var sensor)) {
						continue;
					}

					sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>();
					sensor.Value.LiveDataRouting.Add(new RoutingTarget {
						Target = entry.Target,
						Type = RouteType.LiveDataSubscription
					});
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public override void Add(IEnumerable<Abstract.KeyValuePair<ObjectId, Sensor>> values, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheEntryOptions(options);
			// ReSharper disable once PossibleMultipleEnumeration
			ValidateKeyValuePairs(values);

			this.m_dataLock.EnterWriteLock();

			try {
				// ReSharper disable once PossibleMultipleEnumeration
				foreach(var pair in values) {
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				}

				foreach(var entry in this.m_liveDataEntries) {
					if(!this.m_data.TryGetValue(entry.SensorID, out var sensor)) {
						continue;
					}

					sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>();
					sensor.Value.LiveDataRouting.Add(new RoutingTarget {
						Target = entry.Target,
						Type = RouteType.LiveDataSubscription
					});
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
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
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				}

				foreach(var entry in this.m_liveDataEntries) {
					if(!this.m_data.TryGetValue(entry.SensorID, out var sensor)) {
						continue;
					}

					sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>();
					sensor.Value.LiveDataRouting.Add(new RoutingTarget {
						Target = entry.Target,
						Type = RouteType.LiveDataSubscription
					});
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public override void AddOrUpdate(ObjectId key, Sensor value, CacheEntryOptions options = null)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				base.AddOrUpdate(key, value, options);


				foreach(var entry in this.m_liveDataEntries) {
					if(entry.SensorID != key || !this.m_data.TryGetValue(entry.SensorID, out var sensor)) {
						continue;
					}

					sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>();
					sensor.Value.LiveDataRouting.Add(new RoutingTarget {
						Target = entry.Target,
						Type = RouteType.LiveDataSubscription
					});
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public override void Remove(ObjectId key)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				base.Remove(key);
				this.m_liveSensors.Remove(key);
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public override bool TryRemove(ObjectId key)
		{
			bool rv;
			this.m_dataLock.EnterWriteLock();

			try {
				rv = base.TryRemove(key);
				this.m_liveSensors.Remove(key);
			} finally {
				this.m_dataLock.ExitWriteLock();
			}

			return rv;
		}

		public void AddLiveDataRouting(LiveDataRoute route)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				this.m_liveSensors.Add(route.SensorID);
				this.m_liveDataEntries.Add(route);
				this.m_liveDataSyncEntries.Add(route);

				var sensor = this.m_data[route.SensorID];

				sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>(new RoutingTargetEqualityComparer());
				sensor.Value.LiveDataRouting.Add(new RoutingTarget { Target = route.Target, Type = RouteType.LiveDataSubscription });
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void RemoveLiveDataRouting(LiveDataRoute route)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				var sensor = this.m_data[route.SensorID];
				sensor.Value.LiveDataRouting?.Remove(new RoutingTarget { Target = route.Target, Type = RouteType.LiveDataSubscription });

				this.m_liveDataEntries.Remove(route);
				this.m_liveDataSyncEntries.Remove(route);

				if(sensor.Value.LiveDataRouting != null && sensor.Value.LiveDataRouting!.All(x => x.Type != RouteType.LiveDataSubscription)) {
					this.m_liveSensors.Remove(route.SensorID);
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void SyncLiveDataRoutes(ICollection<LiveDataRoute> data)
		{
			this.m_dataLock.EnterWriteLock();

			try {
				this.m_liveDataSyncEntries.UnionWith(data);
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		public void FlushLiveDataRoutes()
		{
			this.m_dataLock.EnterWriteLock();

			try {
				this.m_liveSensors = new HashSet<ObjectId>();

				this.m_liveDataEntries = this.m_liveDataSyncEntries;
				this.m_liveDataSyncEntries = new HashSet<LiveDataRoute>(new LiveDataRouteEqualityComparer());

				foreach(var entry in this.m_liveDataEntries) {
					if(!this.m_data.TryGetValue(entry.SensorID, out var sensor)) {
						continue;
					}

					if(this.m_liveSensors.Add(entry.SensorID)) {
						sensor.Value.LiveDataRouting ??= new HashSet<RoutingTarget>(new RoutingTargetEqualityComparer());
						sensor.Value.LiveDataRouting.RemoveWhere(x => x.Type == RouteType.LiveDataSubscription);
					}

					sensor.Value.LiveDataRouting.Add(new RoutingTarget {
						Target = entry.Target,
						Type = RouteType.LiveDataSubscription
					});
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
