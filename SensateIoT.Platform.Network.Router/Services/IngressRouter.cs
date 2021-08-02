/*
 * Sensate IoT gRPC router service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Google.Protobuf;
using Grpc.Core;

using JetBrains.Annotations;
using Prometheus;

using SensateIoT.Platform.Router.Contracts.RPC;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Converters;
using SensateIoT.Platform.Router.Common.Validators;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;
using Measurement = SensateIoT.Platform.Router.Data.DTO.Measurement;

namespace SensateIoT.Platform.Network.Router.Services
{
	[UsedImplicitly]
	public class IngressRouter : Platform.Router.Contracts.Services.IngressRouter.IngressRouterBase
	{
		private readonly IQueue<IPlatformMessage> m_queue;
		private readonly ILogger<IngressRouter> m_logger;
		private readonly Counter m_measurementRequests;
		private readonly Counter m_messageRequests;
		private readonly Histogram m_duration;

		public IngressRouter(IQueue<IPlatformMessage> queue, ILogger<IngressRouter> logger)
		{
			this.m_queue = queue;
			this.m_logger = logger;
			this.m_measurementRequests = Metrics.CreateCounter("router_measurement_requests_total", "Total amount of measurement routing requests.");
			this.m_messageRequests = Metrics.CreateCounter("router_message_requests_total", "Total amount of message routing requests.");
			this.m_duration = Metrics.CreateHistogram("router_ingress_request_duration_seconds", "Histogram of egress routing duration.");
		}

		public override Task<RoutingResponse> EnqueueMeasurement(Platform.Router.Contracts.DTO.Measurement request, ServerCallContext context)
		{
			RoutingResponse response;

			using(this.m_duration.NewTimer()) {
				this.m_logger.LogDebug("Received measurement routing request from sensor {sensorId}.",
									   request.SensorID);

				try {
					var count = 0;
					var dto = MeasurementProtobufConverter.Convert(request);

					if(MeasurementValidator.Validate(dto)) {
						this.m_queue.Add(dto);
						this.m_measurementRequests.Inc();

						count += 1;
					}

					response = new RoutingResponse {
						Count = count,
						Message = "Measurements queued.",
						ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
					};
				} catch(FormatException) {
					this.m_logger.LogWarning("Received messages from an invalid sensor (ID): {sensorID}",
											 request.SensorID);

					response = new RoutingResponse {
						Count = 0,
						Message = "Measurements not queued. Invalid sensor ID.",
						ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
					};
				}
			}

			return Task.FromResult(response);
		}

		public override Task<RoutingResponse> EnqueueMessage(TextMessage request, ServerCallContext context)
		{
			RoutingResponse response;

			using(this.m_duration.NewTimer()) {
				this.m_logger.LogDebug("Received message routing request from sensor {sensorId}.", request.SensorID);

				try {
					var count = 0;
					var dto = MessageProtobufConverter.Convert(request);

					if(MessageValidator.Validate(dto)) {
						this.m_queue.Add(dto);
						this.m_messageRequests.Inc();
						count += 1;
					}

					response = new RoutingResponse {
						Count = count,
						Message = "Message queued.",
						ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
					};
				} catch(FormatException) {
					this.m_logger.LogWarning("Received messages from an invalid sensor (ID): {sensorID}",
											 request.SensorID);

					response = new RoutingResponse {
						Count = 0,
						Message = "Messages not queued. Invalid sensor ID.",
						ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
					};
				}
			}

			return Task.FromResult(response);
		}

		public override Task<RoutingResponse> EnqueueBulkMeasurements(MeasurementData request, ServerCallContext context)
		{
			RoutingResponse response;

			using(this.m_duration.NewTimer()) {
				this.m_logger.LogDebug("Bulk ingress request with {count} measurements.", request.Measurements.Count);

				try {
					var dto = new List<Measurement>();

					foreach(var raw in request.Measurements) {
						var measurement = MeasurementProtobufConverter.Convert(raw);

						if(MeasurementValidator.Validate(measurement)) {
							dto.Add(measurement);
						}
					}

					this.m_queue.AddRange(dto);
					this.m_measurementRequests.Inc();

					response = new RoutingResponse {
						Count = dto.Count,
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

		public override Task<RoutingResponse> EnqueueBulkMessages(TextMessageData request, ServerCallContext context)
		{
			RoutingResponse response;

			using(this.m_duration.NewTimer()) {
				this.m_logger.LogDebug("Bulk ingress request with {count} messages.", request.Messages.Count);

				try {
					var dto = new List<Message>();

					foreach(var raw in request.Messages) {
						var message = MessageProtobufConverter.Convert(raw);

						if(MessageValidator.Validate(message)) {
							dto.Add(message);
						}
					}

					this.m_queue.AddRange(dto);
					this.m_messageRequests.Inc();

					response = new RoutingResponse {
						Count = dto.Count,
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
