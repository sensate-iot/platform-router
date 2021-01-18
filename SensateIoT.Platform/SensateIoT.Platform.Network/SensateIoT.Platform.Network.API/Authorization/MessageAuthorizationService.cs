/*
 * Measurement authorization handler.
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
using Microsoft.Extensions.DependencyInjection;

using Google.Protobuf.WellKnownTypes;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.API.Authorization
{
	public class MessageAuthorizationService : AbstractAuthorizationHandler<JsonMessage>, IMessageAuthorizationService
	{
		private readonly IHashAlgorithm m_algo;
		private readonly IRouterClient m_router;
		private readonly ILogger<MessageAuthorizationService> m_logger;
		private readonly IServiceProvider m_provider;

		public MessageAuthorizationService(IServiceProvider provider, IHashAlgorithm algo, IRouterClient client, ILogger<MessageAuthorizationService> logger)
		{
			this.m_logger = logger;
			this.m_algo = algo;
			this.m_provider = provider;
			this.m_router = client;
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

			messages = messages.OrderBy(m => m.Item1.SensorId).ToList();
			var data = await this.BuildMeasurementList(messages).ConfigureAwait(false);
			var remaining = data.Count;
			var index = 0;
			var tasks = new List<Task>();

			if(remaining <= 0) {
				return 0;
			}

			while(remaining > 0) {
				var batch = Math.Min(PartitionSize, remaining);

				tasks.Add(this.SendBatchToRouterAsync(index, remaining, data));
				index += batch;
				remaining -= batch;
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
			return data.Count;
		}

		private async Task SendBatchToRouterAsync(int start, int count, IList<TextMessage> messages)
		{
			var data = new TextMessageData();

			for(var idx = start; idx < start + count; idx++) {
				var message = messages[idx];
				data.Messages.Add(message);
			}

			var result = await this.m_router.RouteAsync(data, default).ConfigureAwait(false);
			this.m_logger.LogInformation("Sent {inputCount} messages to the router. {outputCount} " +
										 "messages have been accepted. Router response ID: {responseID}.",
										 data.Messages.Count, result.Count, new Guid(result.ResponseID.Span));
		}

		private async Task<IList<TextMessage>> BuildMeasurementList(IEnumerable<JsonMessage> messages)
		{
			Sensor sensor = null;
			var data = new List<TextMessage>();

			using var scope = this.m_provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

			foreach(var message in messages) {
				try {
					if(sensor == null || sensor.InternalId != message.Item1.SensorId) {
						sensor = await repo.GetAsync(message.Item1.SensorId).ConfigureAwait(false);
					}

					if(sensor == null || !this.AuthorizeMessage(message, sensor)) {
						continue;
					}

					if(message.Item1.Timestamp == DateTime.MinValue) {
						message.Item1.Timestamp = DateTime.UtcNow;
					}

					var m = new TextMessage {
						SensorID = sensor.InternalId.ToString(),
						Latitude = decimal.ToDouble(message.Item1.Latitude),
						Longitude = decimal.ToDouble(message.Item1.Longitude),
						Timestamp = Timestamp.FromDateTime(message.Item1.Timestamp),
						Data = message.Item1.Data,
						Encoding = (int)message.Item1.Encoding
					};

					data.Add(m);
				} catch(Exception ex) {
					this.m_logger.LogInformation(ex, "Unable to process message: {message}", ex.InnerException?.Message);
				}
			}

			return data;
		}

		protected override bool AuthorizeMessage(JsonMessage message, Sensor sensor)
		{
			var match = this.m_algo.GetMatchRegex();
			var search = this.m_algo.GetSearchRegex();

			if(match.IsMatch(message.Item1.Secret)) {
				var length = message.Item1.Secret.Length - SecretSubStringOffset;
				var hash = HexToByteArray(message.Item1.Secret.Substring(SecretSubStringStart, length));
				var json = search.Replace(message.Item2, sensor.Secret, 1);
				var binary = Encoding.ASCII.GetBytes(json);
				var computed = this.m_algo.ComputeHash(binary);

				if(!CompareHashes(computed, hash)) {
					return false;
				}
			} else {
				return message.Item1.Secret == sensor.Secret;
			}

			return true;
		}
	}
}
