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

using JetBrains.Annotations;
using Prometheus;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Converters;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Contracts.RPC;
using SensateIoT.Platform.Router.Data.Abstract;

namespace SensateIoT.Platform.Network.Router.Services
{
	[UsedImplicitly]
	public class EgressRouter : Platform.Router.Contracts.Services.EgressRouter.EgressRouterBase
	{
		private readonly IQueue<IPlatformMessage> m_queue;
		private readonly ILogger<EgressRouter> m_logger;
		private readonly Counter m_requests;
		private readonly Histogram m_duration;

		public EgressRouter(IQueue<IPlatformMessage> queue, ILogger<EgressRouter> logger)
		{
			this.m_queue = queue;
			this.m_logger = logger;
			this.m_requests = Metrics.CreateCounter("router_egress_requests_total", "Total amount of routing egress traffic.");
			this.m_duration = Metrics.CreateHistogram("router_egress_request_duration_seconds", "Histogram of egress routing duration.");
		}

		public override Task<RoutingResponse> EnqueueBulkControlMessages(ControlMessageData request, ServerCallContext context)
		{
			RoutingResponse response;

			using(this.m_duration.NewTimer()) {
				this.m_logger.LogDebug("Bulk egress request with {count} messages.", request.Messages.Count);

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
			}

			return Task.FromResult(response);
		}

		public override Task<RoutingResponse> EnqueueControlMessage(ControlMessage request, ServerCallContext context)
		{
			RoutingResponse response;

			using(this.m_duration.NewTimer()) {
				this.m_logger.LogDebug("Received egress routing request for sensor {sensorId}.", request.SensorID);

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
			}

			return Task.FromResult(response);
		}

	}
}
