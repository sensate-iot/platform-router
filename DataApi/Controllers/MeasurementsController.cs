/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json.Linq;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.DataApi.Models;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Storage;
using SensateService.Models;
using SensateService.Models.Generic;
using SensateService.Models.Json.Out;
using SensateService.Services;

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class MeasurementsController : AbstractDataController
	{
		private readonly IMeasurementCache m_cache;
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorService m_sensorService;

		public MeasurementsController(IMeasurementCache cache, IMeasurementRepository measurements,
			ISensorService sensorService, ISensorLinkRepository links,
			ISensorRepository sensors, IHttpContextAccessor ctx) : base(ctx, sensors, links)
		{
			this.m_cache = cache;
			this.m_measurements = measurements;
			this.m_sensorService = sensorService;
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
		public async Task<IActionResult> Get(string bucketId, int index = -1)
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

			auth |= await this.IsLinkedSensor(measurement.SensorId.ToString()).AwaitBackground();

			if(!auth) {
				return this.Unauthorized();
			}

			return this.Ok(measurement);
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Filter([FromBody] Filter filter)
		{
			var status = new Status();
			IEnumerable<MeasurementsQueryResult> result;

			if(filter.SensorIds == null || filter.SensorIds.Count <= 0) {
				status.ErrorCode = ReplyCode.BadInput;
				status.Message = "Sensor ID list cannot be empty!";

				return this.UnprocessableEntity(status);
			}

			if(filter.Skip == null) {
				filter.Skip = -1;
			}

			if(filter.Limit == null) {
				filter.Limit = -1;
			}

			var raw = await this.m_sensorService.GetSensorsAsync(this.CurrentUser).AwaitBackground();
			var sensors = raw.ToList();
			var filtered = sensors.Where(x => filter.SensorIds.Contains(x.InternalId.ToString())).ToList();

			if(filtered.Count <= 0) {
				status.Message = "No sensors available!";
				status.ErrorCode = ReplyCode.NotAllowed;

				return this.UnprocessableEntity(status);
			}

			if(filter.Latitude != null & filter.Longitude != null && filter.Radius != null && filter.Radius.Value > 0) {
				var coords = new GeoJson2DGeographicCoordinates(filter.Longitude.Value, filter.Latitude.Value);
				result = await this.m_measurements
					.GetMeasurementsNearAsync(filtered, filter.Start, filter.End, coords, filter.Radius.Value,
						filter.Skip.Value, filter.Limit.Value).AwaitBackground();
			} else {
				result = await this.m_measurements
					.GetMeasurementsBetweenAsync(filtered, filter.Start, filter.End, filter.Skip.Value, filter.Limit.Value).AwaitBackground();
			}

			return this.Ok(result);
		}

		[HttpGet]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Get([FromQuery] string sensorId, [FromQuery] DateTime start, [FromQuery] DateTime end,
			[FromQuery] double? longitude, [FromQuery] double? latitude, [FromQuery] int? radius,
			[FromQuery] int skip = -1, [FromQuery] int limit = -1)
		{
			var sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();

			if(sensor == null) {
				return this.NotFound();
			}

			var linked = await this.IsLinkedSensor(sensorId).AwaitBackground();

			if(!this.AuthenticateUserForSensor(sensor, false) && !linked) {
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
