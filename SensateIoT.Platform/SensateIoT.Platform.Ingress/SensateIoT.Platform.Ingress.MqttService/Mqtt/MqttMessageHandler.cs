/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using SensateIoT.Platform.Ingress.Common.MQTT;
using SensateIoT.Platform.Ingress.Common.Options;
using SensateIoT.Platform.Ingress.DataAccess.Abstract;

namespace SensateIoT.Platform.Ingress.MqttService.Mqtt
{
	public class MqttMessageHandler : IMqttHandler
	{
		private readonly ILogger<MqttMessageHandler> m_logger;
		private readonly IHttpClientFactory m_factory;
		private readonly IServiceProvider m_provider;
		private readonly GatewaySettings m_settings;

		public MqttMessageHandler(IHttpClientFactory factory,
								  IServiceProvider provider,
								  IOptions<GatewaySettings> settings,
								  ILogger<MqttMessageHandler> logger)
		{
			this.m_logger = logger;
			this.m_factory = factory;
			this.m_settings = settings.Value;
			this.m_provider = provider;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			try {
				this.m_logger.LogInformation("Received message via MQTT.");
				using var scope = this.m_provider.CreateScope();

				var repo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
				var jtoken = JToken.Parse(message);
				var rawId = jtoken.SelectToken("sensorId") ?? jtoken.SelectToken("SensorId");

				if(rawId == null) {
					this.m_logger.LogWarning("Message has no sensor ID.");
					return;
				}

				var sensorId = ObjectId.Parse(rawId!.ToString());
				var sensor = await repo.GetSensorAsync(sensorId).ConfigureAwait(false);
				var client = this.m_factory.CreateClient();
				var request = new HttpRequestMessage(HttpMethod.Post, string.Empty) {
					Content = new StringContent(message, Encoding.UTF8, "application/json")
				};

				request.Headers.Add("X-ApiKey", sensor.Secret);

				client.BaseAddress = new Uri(this.m_settings.Messages);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var result = await client.SendAsync(request, ct).ConfigureAwait(false);
				var content = await result.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

				if(!result.IsSuccessStatusCode) {
					this.m_logger.LogDebug("Unable to proxy message: " + Environment.NewLine + "{response}", content);
				}
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Error: {ex.Message}");
				this.m_logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}
	}
}

