/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using SensateIoT.API.Common.ApiCore.Attributes;
using SensateIoT.API.Common.ApiCore.Controllers;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Authorization;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Core.Services.DataProcessing;
using SensateIoT.API.Common.Data.Dto.Generic;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;
using SensateService.Api.DataApi.Dto;

namespace SensateService.Api.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("data/v1/[controller]")]
	public class MeasurementsController : AbstractDataController
	{
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorService m_sensorService;
		private readonly ILogger<MeasurementsController> m_logger;
		private readonly IMeasurementAuthorizationProxyCache m_proxy;

		public MeasurementsController(IMeasurementRepository measurements,
									  ISensorService sensorService,
									  ISensorLinkRepository links,
									  ISensorRepository sensors,
									  IApiKeyRepository keys,
									  IMeasurementAuthorizationProxyCache proxy,
									  ILogger<MeasurementsController> logger,
									  IHttpContextAccessor ctx) : base(ctx, sensors, links, keys)
		{
			this.m_measurements = measurements;
			this.m_sensorService = sensorService;
			this.m_logger = logger;
			this.m_proxy = proxy;
		}

		[HttpPost]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status202Accepted)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromQuery] bool bulk = false)
		{
			var status = new Status();

			try {
				using var reader = new StreamReader(this.Request.Body);
				var raw = await reader.ReadToEndAsync();

				if(!(this.HttpContext.Items["ApiKey"] is SensateApiKey)) {
					return this.Forbid();
				}

				status.ErrorCode = ReplyCode.Ok;
				status.Message = "Measurement queued!";

				if(bulk) {
					this.m_proxy.AddMessages(raw);
				} else {
					this.m_proxy.AddMessage(raw);
				}

				return this.Accepted(status);
			} catch(JsonException) {
				status.Message = "Unable to parse measurements";
				status.ErrorCode = ReplyCode.BadInput;
				return this.UnprocessableEntity(status);
			} catch(Exception ex) {
				status.Message = "Unable to handle request";
				status.ErrorCode = ReplyCode.UnknownError;
				this.m_logger.LogInformation($"Unable to process measurement: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.StatusCode(StatusCodes.Status500InternalServerError, status);
			}
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

		[HttpPost("filter")]
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

			filter.Skip ??= -1;
			filter.Limit ??= -1;

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

			if(filter.End == DateTime.MinValue) {
				filter.End = DateTime.MaxValue;
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

			if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground() && !linked) {
				return this.Unauthorized();
			}

			if(end == DateTime.MinValue) {
				end = DateTime.MaxValue;
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

				if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground()) {
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
