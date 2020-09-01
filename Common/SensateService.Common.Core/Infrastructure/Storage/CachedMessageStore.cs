/*
 * Cached measurement store implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using MessageData = SensateService.Common.Data.Dto.Protobuf.TextMessageData;

namespace SensateService.Infrastructure.Storage
{
	public class CachedMessageStore : ICachedMessageStore
	{
		public static event OnMessagesReceived MessagesReceived;

		private SpinLockWrapper m_lock;
		private readonly IServiceProvider m_provider;
		private List<string> m_data;
		private readonly ILogger<CachedMessageStore> m_logger;

		private const int InitialListSize = 512;
		private const int DatabaseTimeout = 20;

		public CachedMessageStore(IServiceProvider provider, ILogger<CachedMessageStore> logger)
		{
			this.m_provider = provider;
			this.m_logger = logger;
			this.m_lock = new SpinLockWrapper();

			this.m_lock.Lock();
			this.m_data = new List<string>(InitialListSize);
			this.m_lock.Unlock();
		}

		public Task StoreAsync(string obj)
		{
			this.m_lock.Lock();
			this.m_data.Add(obj);
			this.m_lock.Unlock();

			return Task.CompletedTask;
		}

		private static async Task IncrementStatistics(ISensorStatisticsRepository stats, ICollection<MessageStatsUpdate> data, CancellationToken token)
		{
			var tasks = new Task[data.Count];

			for(var idx = 0; idx < data.Count; idx++) {
				var entry = data.ElementAt(idx);
				tasks[idx] = stats.IncrementManyAsync(entry.SensorId, RequestMethod.MqttTcp, entry.Count, token);
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}

		private async Task InvokeEventHandlersAsync(IList<Message> messages, CancellationToken token)
		{
			Delegate[] delegates;
			DataReceivedEventArgs args;

			if(MessagesReceived == null) {
				return;
			}

			delegates = MessagesReceived.GetInvocationList();

			if(delegates.Length <= 0) {
				return;
			}

			var task = Task.Run(messages.Compress, token);

			args = new DataReceivedEventArgs(token) {
				Compressed = await task.AwaitBackground()
			};

			await MessagesReceived.Invoke(this, args).AwaitBackground();
		}

		public async Task<long> ProcessMessagesAsync()
		{
			long count;

			this.m_lock.Lock();
			if(this.m_data.Count <= 0L) {
				this.m_lock.Unlock();
				return 0;
			}

			this.SwapQueues(out var raw_q);
			this.m_lock.Unlock();

			using var scope = this.m_provider.CreateScope();
			var messagesdb = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
			var statsdb = scope.ServiceProvider.GetRequiredService<ISensorStatisticsRepository>();
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeout));

			try {
				var asyncio = new Task[3];
				var messages = DeflateMessages(raw_q).ToList();
				var stats = messages.GroupBy(x => x.SensorId)
					.Select(g => new MessageStatsUpdate {
						SensorId = g.Key,
						Count = g.Count()
					});


				asyncio[0] = IncrementStatistics(statsdb, stats.ToList(), cts.Token);
				asyncio[1] = messagesdb.CreateRangeAsync(messages, cts.Token);
				asyncio[2] = this.InvokeEventHandlersAsync(messages, cts.Token);

				count = messages.Count;
				await Task.WhenAll(asyncio).AwaitBackground();
			} catch(DatabaseException e) {
				cts.Cancel(false);

				this.m_logger.LogInformation($"Measurement database error: {e.Message}");
				this.m_logger.LogDebug(e.StackTrace);

				count = 0;
			} catch(Exception e) {
				cts.Cancel(false);

				this.m_logger.LogInformation($"Bulk measurement I/O error: {e.Message}");
				this.m_logger.LogDebug(e.StackTrace);

				throw new CachingException("Unable to store measurements or statistics!", "MeasurementCache", e);
			} finally {
				raw_q.Clear();
			}

			return count;
		}

		private void SwapQueues(out List<string> data)
		{
			data = this.m_data;
			this.m_data = new List<string>(data.Count);
		}

		private static IEnumerable<Message> DeflateMessages(IEnumerable<string> data)
		{
			var rv = new List<Message>();

			foreach(var b64s in data) {
				var bytes = Convert.FromBase64String(b64s);
				using var to = new MemoryStream();
				using var from = new MemoryStream(bytes);
				using var gzip = new GZipStream(@from, CompressionMode.Decompress);

				gzip.CopyTo(to);
				var deflated = to.ToArray();
				var protoMessages = MessageData.Parser.ParseFrom(deflated);
				var messages = protoMessages.Messages.Select(x => new Message {
					SensorId = ObjectId.Parse(x.SensorId),
					Timestamp = DateTime.Parse(x.Timestamp),
					Data = x.Data,
					Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(x.Longitude, x.Latitude)),
				});

				rv.AddRange(messages);
			}

			return rv;
		}

		public void Destroy()
		{
			this.m_lock.Lock();
			this.m_data.Clear();
			this.m_lock.Unlock();
		}

		public class MessageStatsUpdate
		{
			public ObjectId SensorId { get; set; }
			public int Count { get; set; }
		}
	}
}
