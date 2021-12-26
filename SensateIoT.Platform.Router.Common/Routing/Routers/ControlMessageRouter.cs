/*
 * Control message router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Prometheus;

using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Common.Services.Abstract;
using SensateIoT.Platform.Router.Common.Services.Processing;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

using ControlMessage = SensateIoT.Platform.Router.Data.DTO.ControlMessage;

namespace SensateIoT.Platform.Router.Common.Routing.Routers
{
	public class ControlMessageRouter : IRouter
	{
		private readonly IPublicRemoteQueue m_publicQueue;
		private readonly ILogger<ControlMessageRouter> m_logger;
		private readonly RoutingQueueSettings m_settings;
		private readonly IAuthorizationService m_authService;
		private readonly Counter m_counter;

		private const string FormatNeedle = "$id";

		public string Name => "Control Message Router";

		public ControlMessageRouter(IPublicRemoteQueue remote,
									IOptions<RoutingQueueSettings> settings,
									ILogger<ControlMessageRouter> logger,
									IAuthorizationService auth)
		{
			this.m_publicQueue = remote;
			this.m_authService = auth;
			this.m_settings = settings.Value;
			this.m_logger = logger;
			this.m_counter = Metrics.CreateCounter("router_controlmessage_messages_routed_total", "Total number of routed control messages.");
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			if(message.Type != MessageType.ControlMessage) {
				return true;
			}

			this.m_counter.Inc();
			return this.ProcessMessage(sensor, message as ControlMessage);
		}

		private bool ProcessMessage(Sensor sensor, ControlMessage message)
		{
			var data = JsonConvert.SerializeObject(message, Formatting.None);

			message.Timestamp = DateTime.UtcNow;
			message.Secret = sensor.SensorKey;
			this.m_authService.SignControlMessage(message, data);

			if(message.Destination == ControlMessageType.Mqtt) {
				data = JsonConvert.SerializeObject(message);
				this.m_publicQueue.Enqueue(data, this.m_settings.ActuatorTopicFormat.Replace(FormatNeedle, sensor.ID.ToString()));
				this.m_logger.LogDebug("Publishing control message: {message}", data);
			}

			return true;
		}
	}
}
