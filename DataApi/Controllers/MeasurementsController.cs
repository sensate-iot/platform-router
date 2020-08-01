/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

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
	[Route("data/v1/[controller]")]
	public class MeasurementsController : AbstractDataController
	{
		private readonly IMeasurementCache m_cache;
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorService m_sensorService;
		private readonly ILogger<MeasurementsController> m_logger;

		public MeasurementsController(IMeasurementCache cache,
									  IMeasurementRepository measurements,
									  ISensorService sensorService,
									  ISensorLinkRepository links,
									  ISensorRepository sensors,
									  ILogger<MeasurementsController> logger,
									  IHttpContextAccessor ctx) : base(ctx, sensors, links)
		{
			this.m_cache = cache;
			this.m_measurements = measurements;
			this.m_sensorService = sensorService;
			this.m_logger = logger;
		}

		[HttpPost("create")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status202Accepted)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] string raw)
		{
			var status = new Status();

			if(!(this.HttpContext.Items["ApiKey"] is SensateApiKey key)) {
				return this.Forbid();
			}

			if(key.Type != ApiKeyType.SensorKey) {
				status.ErrorCode = ReplyCode.NotAllowed;
				status.Message = "Invalid sensor API key!";

				return this.BadRequest(status);
			}

			status.ErrorCode = ReplyCode.Ok;
			status.Message = "Measurement queued!";

			await this.m_cache.StoreAsync(raw);
			return this.Accepted(status);
		}

		[HttpGet("{bucketId}/{index}")]
		[ProducesResponseType(typeof(SingleMeasurement), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
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
		[ProducesResponseType(typeof(IEnumerable<MeasurementsQueryResult>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
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

			var sensors = await this.m_sensorService.GetSensorsAsync(this.CurrentUser).AwaitBackground();
			var filtered = sensors.Values.Where(x => filter.SensorIds.Contains(x.InternalId.ToString())).ToList();

			if(filtered.Count <= 0) {
				status.Message = "No sensors available!";
				status.ErrorCode = ReplyCode.NotAllowed;

				return this.UnprocessableEntity(status);
			}

			OrderDirection direction;

			if(string.IsNullOrEmpty(filter.OrderDirection)) {
				filter.OrderDirection = "";
			}

			direction = filter.OrderDirection switch
			{
				"asc" => OrderDirection.Ascending,
				"desc" => OrderDirection.Descending,
				_ => OrderDirection.None,
			};

			if(filter.Latitude != null & filter.Longitude != null && filter.Radius != null && filter.Radius.Value > 0) {
				var coords = new GeoJson2DGeographicCoordinates(filter.Longitude.Value, filter.Latitude.Value);
				result = await this.m_measurements
					.GetMeasurementsNearAsync(filtered, filter.Start, filter.End, coords, filter.Radius.Value,
						filter.Skip.Value, filter.Limit.Value, direction).AwaitBackground();
			} else {
				result = await this.m_measurements
					.GetMeasurementsBetweenAsync(filtered, filter.Start, filter.End, filter.Skip.Value,
												 filter.Limit.Value, direction).AwaitBackground();
			}

			return this.Ok(result);
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<MeasurementsQueryResult>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Get([FromQuery] string sensorId, [FromQuery] DateTime start, [FromQuery] DateTime end,
			[FromQuery] double? longitude, [FromQuery] double? latitude, [FromQuery] int? radius,
			[FromQuery] int skip = -1, [FromQuery] int limit = -1, [FromQuery] string order = "")
		{
			var sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();
			var orderDirection = order switch
			{
				"asc" => OrderDirection.Ascending,
				"desc" => OrderDirection.Descending,
				_ => OrderDirection.None,
			};

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
					.GetMeasurementsNearAsync(sensor, start, end, coords, maxDist, skip, limit, orderDirection)
					.AwaitBackground();

				return this.Ok(data);
			} else {
				var data = await this.m_measurements.GetBetweenAsync(sensor, start, end,
																	 skip, limit, orderDirection).AwaitBackground();
				return this.Ok(data);
			}
		}

		[HttpDelete]
		[ReadWriteApiKey]
		[ProducesResponseType(204)]
		[ProducesResponseType(401)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> Delete([FromQuery] string sensorId, [FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			Sensor sensor;

			try {
				sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();

				if(sensor == null) {
					return this.NotFound();
				}

				if(!this.AuthenticateUserForSensor(sensor, false)) {
					return this.Unauthorized();
				}

				await this.m_measurements.DeleteBetweenAsync(sensor, start, end).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to delete measurements for sensor {sensorId} between " +
											 $"{start.ToString("u", CultureInfo.InvariantCulture)} and " +
											 $"{end.ToString("u", CultureInfo.InvariantCulture)}: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);
				return this.BadRequest();
			}

			return this.NoContent();
		}
	}
}
