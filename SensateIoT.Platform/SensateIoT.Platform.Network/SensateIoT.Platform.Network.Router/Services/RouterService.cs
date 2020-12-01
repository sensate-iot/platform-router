/*
 * Sensate IoT gRPC router service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Google.Protobuf;
using Grpc.Core;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Contracts.RPC;

namespace SensateIoT.Platform.Network.Router.Services
{
	public class RouterService : Contracts.Services.Router.RouterBase
	{
		private readonly IMessageQueue m_queue;
		private readonly ILogger<RouterService> m_logger;

		public RouterService(IMessageQueue queue, ILogger<RouterService> logger)
		{
			this.m_queue = queue;
			this.m_logger = logger;
		}

		public override Task<RoutingResponse> EnqueueMeasurement(Contracts.DTO.Measurement request,
		                                                         ServerCallContext context)
		{
			RoutingResponse response;

			try {
				var dto = MeasurementProtobufConverter.Convert(request);
				this.m_queue.Add(dto);

				response = new RoutingResponse {
					Count = 1,
					Message = "Measurements queued.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			} catch(FormatException) {
				this.m_logger.LogWarning("Received messages from an invalid sensor (ID): {sensorID}", request.SensorID);

				response = new RoutingResponse {
					Count = 0,
					Message = "Measurements not queued. Invalid sensor ID.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			}

			return Task.FromResult(response);
		}

		public override Task<RoutingResponse> EnqueueMessage(Contracts.DTO.TextMessage request,
		                                                     ServerCallContext context)
		{
			RoutingResponse response;

			try {
				var dto = MessageProtobufConverter.Convert(request);
				this.m_queue.Add(dto);

				response = new RoutingResponse {
					Count = 1,
					Message = "Message queued.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			} catch(FormatException) {
				this.m_logger.LogWarning("Received messages from an invalid sensor (ID): {sensorID}", request.SensorID);

				response = new RoutingResponse {
					Count = 0,
					Message = "Messages not queued. Invalid sensor ID.",
					ResponseID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
				};
			}

			return Task.FromResult(response);
		}

		public override Task<RoutingResponse> EnqueueBulkMeasurements(Contracts.DTO.MeasurementData request,
		                                                              ServerCallContext context)
		{
			RoutingResponse response;

			try {
				var dto = MeasurementProtobufConverter.Convert(request);
				this.m_queue.AddRange(dto);

				response = new RoutingResponse {
					Count = request.Measurements.Count,
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

		public override Task<RoutingResponse> EnqueueBulkMessages(Contracts.DTO.TextMessageData request,
		                                                          ServerCallContext context)
		{
			RoutingResponse response;

			try {
				var dto = MessageProtobufConverter.Convert(request);
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
