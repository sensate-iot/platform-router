/*
 * Routing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Remote;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Services.Processing
{
	public class RoutingService : BackgroundService 
	{
		/*
		 * Route messages through the platform:
		 *
		 *		1 Check validity;
		 *		2 Trigger routing;
		 *		3 Live data routing;
		 *		4 Forward to storage.
		 */

		private readonly IDataCache m_cache;
		private readonly IMessageQueue m_messages;
		private readonly IInternalRemoteQueue m_internalRemote;
		private readonly IAuthorizationService m_authService;
		private readonly IPublicRemoteQueue m_publicRemote;
		private readonly ILogger<RoutingService> m_logger;
		private readonly RoutingPublishSettings m_settings;

		private const int DequeueCount = 1000;
		private const string FormatNeedle = "$id";

		public RoutingService(IDataCache cache,
		                      IMessageQueue queue,
		                      IInternalRemoteQueue internalRemote,
							  IPublicRemoteQueue publicRemote,
		                      IAuthorizationService auth,
							  IOptions<RoutingPublishSettings> settings,
		                      ILogger<RoutingService> logger)
		{
			this.m_settings = settings.Value;
			this.m_messages = queue;
			this.m_cache = cache;
			this.m_internalRemote = internalRemote;
			this.m_publicRemote = publicRemote;
			this.m_authService = auth;
			this.m_logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			do {
				Sensor sensor = null;

				if(this.m_messages.Count <= 0) {
					try {
						await Task.Delay(TimeSpan.FromMilliseconds(100), token);
					} catch(OperationCanceledException) {
						Console.WriteLine("Routing task cancelled.");
					}

					continue;
				}

				var messages = this.m_messages.DequeueRange(DequeueCount).ToList();
				messages = messages.OrderBy(x => x.SensorID).ToList();

				this.m_logger.LogInformation("Routing {count} messages.", messages.Count);

				var result = Parallel.ForEach(messages, message => {
					if(sensor?.ID != message.SensorID) {
						sensor = this.m_cache.GetSensor(message.SensorID);
					}

					if(sensor == null) {
						return;
					}

					this.RouteMessage(message, sensor);
				});

				if(!result.IsCompleted) {
					this.m_logger.LogWarning("Unable to complete routing messages! Break called at iteration: {iteration}.", result.LowestBreakIteration);
				}
			} while(!token.IsCancellationRequested);
		}

		private void RouteMessage(IPlatformMessage message, Sensor sensor)
		{
			if(message.Type == MessageType.ControlMessage) {
				this.RouteControlMessage(message as ControlMessage, sensor);
			} else {
				message.PlatformTimestamp = DateTime.UtcNow;

				if(sensor.TriggerInformation.HasActions) {
					this.EnqueueToTriggerService(message, sensor.TriggerInformation.IsTextTrigger);
				}

				if(sensor.LiveDataRouting == null || sensor.LiveDataRouting?.Count <= 0) {
					return;
				}

				foreach(var info in sensor.LiveDataRouting) {
					this.EnqueueTo(message, info);
				}
			}
		}

		private void RouteControlMessage(ControlMessage message, Sensor sensor)
		{
			/*
			 * 1. Timestamp the CM;
			 * 2. Sign the control message;
			 * 2. Queue to the correct output queue.
			 */

			message.Timestamp = DateTime.UtcNow;
			message.Secret = sensor.SensorKey;

			var data = JsonConvert.SerializeObject(message, Formatting.None);
			this.m_authService.SignControlMessage(message, data);
			data = JsonConvert.SerializeObject(message);
			this.m_logger.LogDebug("Publishing control message: {message}", data);
			this.m_publicRemote.Enqueue(data, this.m_settings.ActuatorTopicFormat.Replace(FormatNeedle, sensor.ID.ToString()));
		}

		private void EnqueueToTriggerService(IPlatformMessage message, bool isText)
		{
			switch(message.Type) {
			case MessageType.Measurement when isText:
				return;

			case MessageType.Measurement:
				this.m_internalRemote.EnqueueMeasurementToTriggerService(message);
				break;

			case MessageType.Message:
				this.m_internalRemote.EnqueueToMessageTriggerService(message);
				break;

			default:
				throw new ArgumentException($"Unable to enqueue message of type {message.Type}.");
			}
		}

		private void EnqueueTo(IPlatformMessage message, RoutingTarget target)
		{
			switch(message.Type) {
			case MessageType.Message:
				this.m_internalRemote.EnqueueMessageToTarget(message, target);
				break;

			case MessageType.Measurement:
				this.m_internalRemote.EnqueueMeasurementToTarget(message, target);
				break;

			default:
				throw new ArgumentException($"Unable to enqueue message of type {message.Type}.");
			}
		}
	}
}
