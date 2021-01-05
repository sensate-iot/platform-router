/*
 * Routing client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Grpc.Core;

using SensateIoT.Platform.Network.API.Config;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Contracts.RPC;
using SensateIoT.Platform.Network.Contracts.Services;

namespace SensateIoT.Platform.Network.API.Services
{
	public class RouterClient : IRouterClient
	{
		private readonly Channel m_channel;

		public RouterClient(IOptions<RouterConfig> config)
		{
			this.m_channel = new Channel(config.Value.Hostname, config.Value.Port, ChannelCredentials.Insecure);
		}

		public async Task<RoutingResponse> RouteAsync(MeasurementData data, CancellationToken ct)
		{
			var client = new IngressRouter.IngressRouterClient(this.m_channel);
			return await client.EnqueueBulkMeasurementsAsync(data, cancellationToken: ct);
		}

		public async Task<RoutingResponse> RouteAsync(TextMessageData data, CancellationToken ct)
		{
			var client = new IngressRouter.IngressRouterClient(this.m_channel);
			return await client.EnqueueBulkMessagesAsync(data, cancellationToken: ct);
		}

		public async Task<RoutingResponse> RouteAsync(ControlMessage data, CancellationToken ct)
		{
			var client = new EgressRouter.EgressRouterClient(this.m_channel);
			return await client.EnqueueControlMessageAsync(data, cancellationToken: ct);
		}
	}
}
