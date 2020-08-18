/*
 * Blob API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Api.BlobApi.Models;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Services;
using SensateService.Services.Settings;
using SensateService.Settings;

namespace SensateService.Api.BlobApi.Controllers
{
	[Produces("application/json")]
	[Route("storage/v1/[controller]")]
	public class BlobsController : AbstractDataController
	{
		private readonly ILogger<BlobsController> m_logger;
		private readonly IBlobRepository m_blobs;
		private readonly BlobStorageSettings m_settings;
		private readonly IMqttPublishService m_publisher;
		private readonly InternalMqttServiceOptions m_mqttOptions;

		public BlobsController(IHttpContextAccessor ctx, ISensorRepository sensors, IBlobRepository blobs,
			ISensorLinkRepository links,
			IOptions<BlobStorageSettings> blobOptions, ILogger<BlobsController> logger,
			IMqttPublishService publisher,
			IApiKeyRepository keys,
			IOptions<InternalMqttServiceOptions> mqttOptions
			) : base(ctx, sensors, links, keys)
		{
			this.m_settings = blobOptions.Value;
			this.m_blobs = blobs;
			this.m_logger = logger;
			this.m_publisher = publisher;
			this.m_mqttOptions = mqttOptions.Value;
		}

		private string CreatePath(string sensorId)
		{
			return $"{this.m_settings.StoragePath}{Path.DirectorySeparatorChar}{sensorId}";
		}

		[HttpPost]
		[ValidateModel]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromForm] FileUploadForm upload)
		{
			var file = upload.File;

			if(file == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid file upload!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			this.m_logger.LogDebug($"Creating file/blob: {file.Name}");

			var blob = new Blob {
				SensorId = upload.SensorId,
				FileName = upload.Name,
				Timestamp = DateTime.UtcNow,
				Path = this.CreatePath(upload.SensorId),
				StorageType = StorageType.FileSystem,
				FileSize = upload.File.Length
			};

			var auth = await this.AuthenticateUserForSensor(upload.SensorId, true).AwaitBackground();

			if(!auth) {
				return this.Forbid();
			}

			try {
				Directory.CreateDirectory(blob.Path);

				await using var stream = new FileStream($"{blob.Path}{Path.DirectorySeparatorChar}{blob.FileName}", FileMode.Create);
				await upload.File.CopyToAsync(stream).AwaitBackground();
				await this.m_blobs.CreateAsync(blob).AwaitBackground();

				await this.m_publisher.PublishOnAsync(this.m_mqttOptions.InternalBlobTopic, JsonConvert.SerializeObject(blob), false).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to store blob: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to store blob!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.CreatedAtAction(nameof(Get), new { sensorId = blob.SensorId, fileName = blob.FileName }, blob);
		}

		[HttpGet("{sensorId}/{fileName}")]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Get(string sensorId, string fileName)
		{
			FileStreamResult file;

			try {
				var auth = await this.AuthenticateUserForSensor(sensorId).AwaitBackground();

				if(!auth) {
					return this.Forbid();
				}

				var blob = await this.m_blobs.GetAsync(sensorId, fileName).AwaitBackground();

				if(blob == null) {
					return this.NotFound();
				}

				var filePath = $"{blob.Path}{Path.DirectorySeparatorChar}{blob.FileName}";
				var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				file = this.File(stream, "application/octet-stream");
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to fetch blob: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to fetch blob!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return file;
		}

		[HttpGet]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(PaginationResult<Blob>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> GetAsync(string sensorId, int skip = -1, int limit = -1, string order = "")
		{
			PaginationResult<Blob> blobs;

			var orderDirection = order switch
			{
				"asc" => OrderDirection.Ascending,
				"desc" => OrderDirection.Descending,
				_ => OrderDirection.None,
			};

			if(string.IsNullOrEmpty(sensorId)) {
				var sensors = await this.m_sensors.GetAsync(this.CurrentUser).AwaitBackground();
				blobs = await this.m_blobs.GetRangeAsync(sensors.ToList(), skip, limit, orderDirection).AwaitBackground();
			} else {
				blobs = await this.m_blobs.GetAsync(sensorId, skip, limit, orderDirection).AwaitBackground();
			}

			return this.Ok(blobs);
		}

		[HttpDelete("{sensorId}/{fileName}")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> Delete(string sensorId, string fileName)
		{
			try {
				var auth = await this.AuthenticateUserForSensor(sensorId).AwaitBackground();

				if(!auth) {
					return this.Forbid();
				}

				var result = await this.m_blobs.DeleteAsync(sensorId, fileName).AwaitBackground();

				if(!result) {
					return this.NotFound();
				}

				var path = $"{this.CreatePath(sensorId)}{Path.DirectorySeparatorChar}{fileName}";

				if(System.IO.File.Exists(path)) {
					System.IO.File.Delete(path);
				}
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to fetch blob: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to fetch blob!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.NoContent();
		}

		[HttpGet("{blobId}")]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetById(long blobId)
		{
			FileStreamResult file;

			try {
				var blob = await this.m_blobs.GetAsync(blobId).AwaitBackground();

				if(blob == null) {
					return this.NotFound();
				}

				var auth = await this.AuthenticateUserForSensor(blob.SensorId).AwaitBackground();

				if(!auth) {
					return this.Forbid();
				}

				var filePath = $"{blob.Path}{Path.DirectorySeparatorChar}{blob.FileName}";
				var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				file = this.File(stream, "application/octet-stream");
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to fetch blob: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to fetch blob!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return file;
		}
	}
}

