/*
 * MQTT handler for incoming messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.StorageService.MQTT
{
	public class MqttBulkMessageHandler : IMqttHandler
	{
		private readonly ILogger<MqttBulkMessageHandler> m_logger;
		private readonly IMessageRepository m_messages;

		public MqttBulkMessageHandler(IMessageRepository message, ILogger<MqttBulkMessageHandler> logger)
		{
			this.m_logger = logger;
			this.m_messages = message;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct)
		{
			try {
				var databaseMessages = this.Decompress(message);
				await this.m_messages.CreateRangeAsync(databaseMessages, ct).ConfigureAwait(false);
			} catch(Exception ex) {
				this.m_logger.LogWarning("Unable to store message: {exception} " +
										 "Message content: {message}. " +
										 "Stack trace: ", ex.Message, message, ex.StackTrace);
			}
		}

		private IEnumerable<Message> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(@from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var protoMeasurements = TextMessageData.Parser.ParseFrom(final);
			this.m_logger.LogInformation("Storing {count} messages!", protoMeasurements.Messages.Count);
			return MessageDatabaseConverter.Convert(protoMeasurements);
		}
	}
}
