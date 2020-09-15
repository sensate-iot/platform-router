/*
 * Message authorization handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Protobuf;
using SensateService.Crypto;
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization.Cache;
using SensateService.Infrastructure.Events;

namespace SensateService.Infrastructure.Authorization.Handlers
{
	public class MessageAuthorizationHandler : AbstractAuthorizationHandler<JsonMessage>
	{
		private readonly IHashAlgorithm m_algo;
		private readonly IDataCache m_cache;
		private readonly ILogger<MessageAuthorizationHandler> m_logger;

		public MessageAuthorizationHandler(IHashAlgorithm algo, IDataCache cache, ILogger<MessageAuthorizationHandler> logger)
		{
			this.m_algo = algo;
			this.m_cache = cache;
			this.m_logger = logger;
		}

		public override async Task<int> ProcessAsync()
		{
			List<JsonMessage> messages;

			this.m_lock.Lock();

			try {
				var newList = new List<JsonMessage>();
				messages = this.m_messages;
				this.m_messages = newList;
			} finally {
				this.m_lock.Unlock();
			}

			if(messages == null || messages.Count <= 0) {
				return 0;
			}

			messages = messages.OrderBy(m => m.Message.SensorId).ToList();
			Sensor sensor = null;
			var data = new List<TextMessage>();

			foreach(var message in messages) {
				try {
					if(sensor == null || sensor.Id != message.Message.SensorId) {
						sensor = this.m_cache.GetSensor(message.Message.SensorId);
					}

					if(sensor == null || !this.AuthorizeMessage(message, sensor)) {
						continue;
					}

					if(message.Message.Timestamp == DateTime.MinValue) {
						message.Message.Timestamp = DateTime.UtcNow;
					}

					var m = new TextMessage {
						Latitude = message.Message.Latitude,
						Longitude = message.Message.Longitude,
						SensorId = message.Message.SensorId.ToString(),
						Timestamp = message.Message.Timestamp.ToString("O"),
						Data = message.Message.Data
					};

					data.Add(m);
				} catch(Exception ex) {
					this.m_logger.LogInformation(ex, "Unable to process message: {message}", ex.InnerException?.Message);
				}
			}

			if(data.Count <= 0) {
				return 0;
			}

			var tasks = new List<Task>();
			var rv = data.Count;

			if(data.Count > PartitionSize) {
				var partitions = data.Partition(PartitionSize);
				var measurementData = new TextMessageData();

				foreach(var partition in partitions) {
					measurementData.Messages.AddRange(partition);
				}

				var args = new DataAuthorizedEventArgs { Data = measurementData };

				tasks.Add(AuthorizationCache.InvokeMessageEvent(this, args));
			} else {
				var measurementData = new TextMessageData();
				measurementData.Messages.AddRange(data);

				var args = new DataAuthorizedEventArgs { Data = measurementData };
				tasks.Add(AuthorizationCache.InvokeMessageEvent(this, args));
			}

			await Task.WhenAll(tasks).AwaitBackground();
			return rv;
		}

		protected override bool AuthorizeMessage(JsonMessage message, Sensor sensor)
		{
			var match = this.m_algo.GetMatchRegex();
			var search = this.m_algo.GetSearchRegex();

			if(match.IsMatch(message.Message.Secret)) {
				var length = message.Message.Secret.Length - SecretSubStringOffset;
				var hash = HexToByteArray(message.Message.Secret.Substring(SecretSubStringStart, length));
				var json = search.Replace(message.Json, sensor.Secret, 1);
				var binary = Encoding.ASCII.GetBytes(json);
				var computed = this.m_algo.ComputeHash(binary);

				if(!CompareHashes(computed, hash)) {
					return false;
				}
			} else {
				return message.Message.Secret == sensor.Secret;
			}

			return true;
		}
	}
}
