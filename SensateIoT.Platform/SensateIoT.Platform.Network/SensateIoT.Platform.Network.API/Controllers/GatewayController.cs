/*
 * Message gateway controller.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;
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
		private readonly ILogger<GatewayController> m_logger;

		public GatewayController(IMeasurementAuthorizationService measurementAuth,
								 IMessageAuthorizationService messageAuth,
								 IHttpContextAccessor ctx,
								 ISensorRepository sensors,
								 IApiKeyRepository keys,
								 ISensorLinkRepository links,
								 IBlobRepository blobs,
								 IBlobService blobService,
								 ILogger<GatewayController> logger) : base(ctx, sensors, links, keys)
		{
			this.m_measurementAuthorizationService = measurementAuth;
			this.m_messageAuthorizationService = messageAuth;
			this.m_blobService = blobService;
			this.m_blobs = blobs;
			this.m_logger = logger;
		}

		[HttpPost("blobs")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status201Created)]
		public async Task<IActionResult> CreateBlob([FromForm] FileUploadForm upload)
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
		public async Task<IActionResult> Messages()
		{
			var response = new Response<GatewayResponse>();

			using var reader = new StreamReader(this.Request.Body);
			var raw = await reader.ReadToEndAsync();

			var message = JsonConvert.DeserializeObject<Message>(raw);
			this.m_messageAuthorizationService.AddMessage(new JsonMessage(message, raw));

			response.Data = new GatewayResponse {
				Message = "Measurements received and queued.",
				Queued = 1
			};

			return this.Accepted(response);
		}

		[HttpPost("measurements")]
		[ReadWriteApiKey, ValidateModel]
		public async Task<IActionResult> Measurements()
		{
			var response = new Response<GatewayResponse>();

			using var reader = new StreamReader(this.Request.Body);
			var raw = await reader.ReadToEndAsync();

			var measurement = JsonConvert.DeserializeObject<Measurement>(raw);
			this.m_measurementAuthorizationService.AddMessage(new JsonMeasurement(measurement, raw));

			response.Data = new GatewayResponse {
				Message = "Measurements received and queued.",
				Queued = 1
			};

			return this.Accepted(response);
		}
	}
}
