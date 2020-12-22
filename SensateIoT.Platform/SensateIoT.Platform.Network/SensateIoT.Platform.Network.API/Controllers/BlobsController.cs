/*
 * Blobs controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.API.Controllers
{
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class BlobsController : AbstractApiController
	{
		private readonly IBlobRepository m_blobs;
		private readonly IBlobService m_blobService;

		public BlobsController(IHttpContextAccessor ctx,
		                       ISensorRepository sensors,
		                       ISensorLinkRepository links,
							   IBlobRepository blobs,
							   IBlobService blobService,
		                       IApiKeyRepository keys) : base(ctx, sensors, links, keys)
		{
			this.m_blobs = blobs;
			this.m_blobService = blobService;
		}

		[HttpGet("{sensorId}/{fileName}")]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> GetByFileName(string sensorId, string fileName)
		{
			FileContentResult file;

			var auth = await this.AuthenticateUserForSensor(sensorId).ConfigureAwait(false);

			if(!auth) {
				return this.Forbidden();
			}

			var blob = await this.m_blobs.GetAsync(sensorId, fileName).ConfigureAwait(false);

			if(blob == null) {
				return this.NotFound();
			}

			var contents = await this.m_blobService.ReadAsync(blob).ConfigureAwait(false);
			file = this.File(contents, "application/octet-stream");

			return file;
		}

		[HttpGet]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(PaginationResponse<Blob>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> GetAsync(string sensorId, int skip = -1, int limit = -1)
		{
			IList<Blob> results;

			if(string.IsNullOrEmpty(sensorId)) {
				var sensors = await this.m_sensors.GetAsync(this.CurrentUser.ID).ConfigureAwait(false);
				var data = await this.m_blobs.GetRangeAsync(sensors.ToList(), skip, limit).ConfigureAwait(false);
				results = data.ToList();
			} else {
				var data = await this.m_blobs.GetAsync(sensorId, skip, limit).ConfigureAwait(false);
				results = data.ToList();
			}

			var response = new PaginationResponse<Blob> {
				Data = new PaginationResult<Blob> {
					Count = results.Count,
					Limit = limit,
					Skip = skip,
					Values = results
				}
			};

			return this.Ok(response);
		}

		[HttpDelete("{sensorId}/{fileName}")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> Delete(string sensorId, string fileName)
		{
			var auth = await this.AuthenticateUserForSensor(sensorId).ConfigureAwait(false);

			if(!auth) {
				return this.Forbidden();
			}

			var result = await this.m_blobs.DeleteAsync(sensorId, fileName).ConfigureAwait(false);

			if(result == null) {
				return this.NotFound();
			}

			await this.m_blobService.RemoveAsync(result).ConfigureAwait(false);
			return this.NoContent();
		}

		[HttpGet("{blobId}", Name = "GetBlobById")]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetById(long blobId)
		{
			FileStreamResult file;
			var blob = await this.m_blobs.GetAsync(blobId).ConfigureAwait(false);

			if(blob == null) {
				return this.NotFound();
			}

			var auth = await this.AuthenticateUserForSensor(blob.SensorID).ConfigureAwait(false);

			if(!auth) {
				return this.Forbidden();
			}

			var filePath = $"{blob.Path}{Path.DirectorySeparatorChar}{blob.FileName}";
			var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			file = this.File(stream, "application/octet-stream");

			return file;
		}
	}
}
