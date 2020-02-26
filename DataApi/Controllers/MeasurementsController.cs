/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json.Linq;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Storage;
using SensateService.Models;
using SensateService.Models.Generic;
using SensateService.Models.Json.Out;

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class MeasurementsController : AbstractDataController 
	{
		private readonly IMeasurementCache m_cache;
		private readonly IMeasurementRepository m_measurements;

		public MeasurementsController(IMeasurementCache cache, IMeasurementRepository measurements, ISensorRepository sensors, IHttpContextAccessor ctx) : base(ctx, sensors)
		{
			this.m_cache = cache;
			this.m_measurements = measurements;
		}

		[HttpPost("create")]
		[ReadWriteApiKey]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Create([FromBody] JObject raw)
		{
			Status status = new Status();

			if(!(this.HttpContext.Items["ApiKey"] is SensateApiKey key)) {
				return this.Forbid();
			}

			if(key.Type != ApiKeyType.SensorKey) {
				status.ErrorCode = ReplyCode.NotAllowed;
				status.Message = "Invalid sensor API key!";

				return this.BadRequest(status);
			}

			using var reader = new StreamReader(this.Request.Body);
			var body = await reader.ReadToEndAsync().AwaitBackground();
			var json = JObject.Parse(body);

			status.ErrorCode = ReplyCode.Ok;
			status.Message = "Measurement queued!";

			await this.m_cache.StoreAsync(json, RequestMethod.HttpPost);
			return this.Ok(status);
		}

		[HttpGet("{bucketId}/{index}")]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Get( string bucketId, int index = -1)
		{
			if(bucketId == null && index < 0) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid query",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var idx = new MeasurementIndex { Index = index };

			if(!ObjectId.TryParse(bucketId, out var id)) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid sensor ID",
					ErrorCode = ReplyCode.BadInput
				});
			}

			idx.MeasurementBucketId = id;

			var measurement = await this.m_measurements.GetMeasurementAsync(idx).AwaitBackground();
			var auth = await this.AuthenticateUserForSensor(measurement.SensorId.ToString()).AwaitBackground();

			if(!auth) {
				return this.Unauthorized();
			}

			return this.Ok(measurement);
		}

		[HttpGet]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Get([FromQuery] string sensorId, [FromQuery] DateTime start, [FromQuery] DateTime end,
			[FromQuery] double? longitude, [FromQuery] double? latitude, [FromQuery] int? radius,
			[FromQuery] int skip = -1, [FromQuery] int limit = -1)
		{
			var sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();

			if(!this.AuthenticateUserForSensor(sensor, false)) {
				return this.Unauthorized();
			}

			if(longitude != null && latitude != null) {
				var maxDist = radius ?? 100;
				var coords = new GeoJson2DGeographicCoordinates(longitude.Value, latitude.Value);

				var data = await this.m_measurements
					.GetMeasurementsNearAsync(sensor, start, end, coords, maxDist, skip, limit).AwaitBackground();

				return this.Ok(data);
			} else {
				var data = await this.m_measurements.GetBetweenAsync(sensor, start, end, skip, limit).AwaitBackground();
				return this.Ok(data);
			}

		}
	}
}
