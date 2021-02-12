/*
 * Routing client implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Contracts.Services;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.TriggerService.Settings;

namespace SensateIoT.Platform.Network.TriggerService.Clients
{
	public class RouterClient : IRouterClient
	{
		private readonly Channel m_channel;
		private readonly ILogger<RouterClient> m_logger;

		public RouterClient(IOptions<RouterSettings> settings, ILogger<RouterClient> logger)
		{
			var creds = settings.Value.Secure ? new SslCredentials() : ChannelCredentials.Insecure;

			this.m_channel = new Channel(settings.Value.Host, Convert.ToInt32(settings.Value.Port), creds);
			this.m_logger = logger;
		}

		public async Task RouteControlMessageAsync(ControlMessage msg, ControlMessageType type)
		{
			var controlMessage = new Contracts.DTO.ControlMessage {
				Data = msg.Data,
				Timestamp = Timestamp.FromDateTime(msg.Timestamp),
				SensorID = msg.SensorId.ToString(),
				Destination = Convert.ToInt32(type)
			};

			var client = new EgressRouter.EgressRouterClient(this.m_channel);
			var result = await client.EnqueueControlMessageAsync(controlMessage, Metadata.Empty);

			if(result.Count < 1) {
				var guid = new Guid(result.ResponseID.Span.ToArray());
				this.m_logger.LogWarning("Unable to route actuator message with response ID: {guid}.", guid);
			}
		}
	}
}
