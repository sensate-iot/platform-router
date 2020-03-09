/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttMeasurementHandler : Middleware.MqttHandler
	{
		private readonly IMeasurementCache store;
		private readonly ILogger<MqttMeasurementHandler> logger;

		public MqttMeasurementHandler(IMeasurementCache store, ILogger<MqttMeasurementHandler> logger)
		{
			this.store = store;
			this.logger = logger;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			JObject raw;

			try {
				var reader = new JsonTextReader(new StringReader(message)) { FloatParseHandling = FloatParseHandling.Decimal };

				raw = JObject.Load(reader);
				await this.store.StoreAsync(raw, RequestMethod.MqttTcp).AwaitBackground();
			} catch(CachingException ex) {
				this.logger.LogInformation($"{ex.Key}: {ex.Message}");
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
