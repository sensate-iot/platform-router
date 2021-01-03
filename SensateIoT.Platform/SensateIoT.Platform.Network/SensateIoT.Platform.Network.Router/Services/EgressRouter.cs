/*
 * Egress routing service. This service routes from the internal
 * network the Sensate IoT Network edge.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Google.Protobuf;
using Grpc.Core;
using Prometheus;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Contracts.RPC;

namespace SensateIoT.Platform.Network.Router.Services
{
	public class EgressRouter : Contracts.Services.EgressRouter.EgressRouterBase
	{
		private readonly IMessageQueue m_queue;
		private readonly ILogger<EgressRouter> m_logger;
		private readonly Counter m_requests;

		public EgressRouter(IMessageQueue queue, ILogger<EgressRouter> logger)
		{
			this.m_queue = queue;
			this.m_logger = logger;
			this.m_requests = Metrics.CreateCounter("router_egress_requests_total", "Total amount of routing egress traffic.");
		}

		public override Task<RoutingResponse> EnqueueBulkControlMessages(Contracts.DTO.ControlMessageData request, ServerCallContext context)
		{
			RoutingResponse response;

			try {
				var dto = ControlMessageProtobufConverter.Convert(request);

				this.m_requests.Inc();
				this.m_queue.AddRange(dto);

				response = new RoutingResponse {
					Count = request.Messages.Count,
					Message = "Messages queued.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			} catch(FormatException ex) {
				this.m_logger.LogWarning("Received messages from an invalid sensor. Exception: {exception}", ex);

				response = new RoutingResponse {
					Count = 0,
					Message = "Messages not queued. Invalid sensor ID.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			}

			return Task.FromResult(response);
		}

		public override Task<RoutingResponse> EnqueueControlMessage(Contracts.DTO.ControlMessage request, ServerCallContext context)
		{
			RoutingResponse response;

			try {
				var dto = ControlMessageProtobufConverter.Convert(request);

				this.m_requests.Inc();
				this.m_queue.Add(dto);

				response = new RoutingResponse {
					Count = 1,
					Message = "Messages queued.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			} catch(FormatException ex) {
				this.m_logger.LogWarning("Received messages from an invalid sensor. Exception: {exception}", ex);

				response = new RoutingResponse {
					Count = 0,
					Message = "Messages not queued. Invalid sensor ID.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			}

			return Task.FromResult(response);
		}

	}
}
