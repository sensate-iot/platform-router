/*
 * Message gateway controller.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.Services.Processing;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

using Measurement = SensateIoT.Platform.Network.API.DTO.Measurement;
using Message = SensateIoT.Platform.Network.API.DTO.Message;

namespace SensateIoT.Platform.Network.API.Controllers
{
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class GatewayController : AbstractApiController
	{
		private readonly IMeasurementAuthorizationService m_measurementAuthorizationService;
		private readonly IMessageAuthorizationService m_messageAuthorizationService;
		private readonly IBlobRepository m_blobs;
		private readonly IBlobService m_blobService;
		private readonly IRouterClient m_client;
		private readonly ILogger<GatewayController> m_logger;
		private readonly IAuthorizationService m_auth;

		public GatewayController(IMeasurementAuthorizationService measurementAuth,
								 IMessageAuthorizationService messageAuth,
								 IHttpContextAccessor ctx,
								 ISensorRepository sensors,
								 IApiKeyRepository keys,
								 ISensorLinkRepository links,
								 IBlobRepository blobs,
								 IBlobService blobService,
								 IRouterClient client,
								 IAuthorizationService auth,
								 ILogger<GatewayController> logger) : base(ctx, sensors, links, keys)
		{
			this.m_measurementAuthorizationService = measurementAuth;
			this.m_messageAuthorizationService = messageAuth;
			this.m_blobService = blobService;
			this.m_blobs = blobs;
			this.m_client = client;
			this.m_logger = logger;
			this.m_auth = auth;
		}

		[HttpPost("blobs")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(Blob), StatusCodes.Status201Created)]
		public async Task<IActionResult> CreateBlobAsync([FromForm] FileUploadForm upload)
		{
			var file = upload.File;

			if(file == null) {
				var response = new Response<string>();

				foreach(var error in this.ModelState.Values.SelectMany(modelState => modelState.Errors)) {
					response.AddError(error.ErrorMessage);
				}

				return this.UnprocessableEntity(response);
			}

			this.m_logger.LogDebug($"Creating file/blob: {file.Name}");

			var blob = new Blob {
				SensorID = upload.SensorId,
				FileName = upload.Name,
				Timestamp = DateTime.UtcNow,
				FileSize = Convert.ToInt32(upload.File.Length)
			};

			var auth = await this.AuthenticateUserForSensor(upload.SensorId, true).ConfigureAwait(false);

			if(!auth) {
				return this.Forbidden();
			}

			await using var mstream = new MemoryStream();
			await upload.File.CopyToAsync(mstream);
			await this.m_blobService.StoreAsync(blob, mstream.ToArray()).ConfigureAwait(false);
			blob = await this.m_blobs.CreateAsync(blob).ConfigureAwait(false);

			return this.CreatedAtRoute("GetBlobById", new { Controller = "blobs", blobId = blob.ID }, blob);
		}

		[HttpPost("messages")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<GatewayResponse>), StatusCodes.Status202Accepted)]
		public async Task<IActionResult> EnqueueMessageAsync([FromBody] Message message)
		{
			var response = new Response<GatewayResponse>();

			var raw = await this.PeekRequestBodyAsString().ConfigureAwait(false);
			this.m_messageAuthorizationService.AddMessage(new JsonMessage(message, raw));

			response.Data = new GatewayResponse {
				Message = "Messages received and queued.",
				Queued = 1
			};

			return this.Accepted(response);
		}

		[HttpPost("measurements")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<GatewayResponse>), StatusCodes.Status202Accepted)]
		public async Task<IActionResult> EnqueueMeasurement([FromBody] Measurement measurement)
		{
			var response = new Response<GatewayResponse>();

			var raw = await this.PeekRequestBodyAsString().ConfigureAwait(false);
			this.m_measurementAuthorizationService.AddMessage(new JsonMeasurement(measurement, raw));

			response.Data = new GatewayResponse {
				Message = "Measurements received and queued.",
				Queued = 1
			};

			return this.Accepted(response);
		}

		[HttpPost("actuators")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<GatewayResponse>), StatusCodes.Status202Accepted)]
		public async Task<IActionResult> EnqueueControlMessageAsync([FromBody] ActuatorMessage message)
		{
			var response = new Response<GatewayResponse>();

			if(!ObjectId.TryParse(message.SensorId, out var id)) {
				response.AddError("Invalid sensor ID.");
				return this.BadRequest(response);
			}

			var sensor = await this.m_sensors.GetAsync(id).ConfigureAwait(false);
			var auth = await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false);

			if(!auth) {
				response.AddError($"Sensor {message.SensorId} is not authorized for user {this.m_currentUserId}");
				return this.Unauthorized(response);
			}

			var controlMessage = new Data.DTO.ControlMessage {
				Data = message.Data,
				Destination = ControlMessageType.Mqtt,
				PlatformTimestamp = DateTime.UtcNow,
				Timestamp = DateTime.UtcNow,
				Secret = sensor.Secret,
				SensorId = id
			};

			var json = JsonConvert.SerializeObject(controlMessage);
			this.m_auth.SignControlMessage(controlMessage, json);

			await this.m_client.RouteAsync(ControlMessageProtobufConverter.Convert(controlMessage), CancellationToken.None)
				.ConfigureAwait(false);

			response.Data = new GatewayResponse {
				Message = "Control message received and queued.",
				Queued = 1
			};

			return this.Ok(response);
		}

		private async Task<string> PeekRequestBodyAsString()
		{
			try {
				var buffer = new byte[Convert.ToInt32(this.Request.ContentLength)];
				this.Request.Body.Position = 0;

				await this.Request.Body.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None).ConfigureAwait(false);
				return Encoding.UTF8.GetString(buffer);
			} finally {
				this.Request.Body.Position = 0;
			}
		}
	}
}
