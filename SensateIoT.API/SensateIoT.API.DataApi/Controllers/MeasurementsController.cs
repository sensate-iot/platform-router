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

using SensateIoT.API.Common.ApiCore.Attributes;
using SensateIoT.API.Common.ApiCore.Controllers;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Core.Services.DataProcessing;
using SensateIoT.API.Common.Data.Converters;
using SensateIoT.API.Common.Data.Dto.Generic;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.DataApi.Dto;

using MeasurementsQueryResult = SensateIoT.API.Common.Data.Models.MeasurementsQueryResult;
using MQR = SensateIoT.API.Common.Data.Dto.Generic.MeasurementsQueryResult;

namespace SensateIoT.API.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("data/v1/[controller]")]
	public class MeasurementsController : AbstractDataController
	{
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorService m_sensorService;
		private readonly ILogger<MeasurementsController> m_logger;

		public MeasurementsController(IMeasurementRepository measurements,
									  ISensorService sensorService,
									  ISensorLinkRepository links,
									  ISensorRepository sensors,
									  IApiKeyRepository keys,
									  ILogger<MeasurementsController> logger,
									  IHttpContextAccessor ctx) : base(ctx, sensors, links, keys)
		{
			this.m_measurements = measurements;
			this.m_sensorService = sensorService;
			this.m_logger = logger;
		}

		[HttpPost("filter")]
		[ProducesResponseType(typeof(IEnumerable<MQR>), StatusCodes.Status200OK)]
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
				var coords = new GeoJsonPoint {
					Latitude = filter.Latitude.Value,
					Longitude = filter.Longitude.Value
				};
				result = await this.m_measurements
					.GetMeasurementsNearAsync(filtered, filter.Start, filter.End, coords, filter.Radius.Value,
						filter.Skip.Value, filter.Limit.Value, direction).AwaitBackground();
			} else {
				result = await this.m_measurements
					.GetMeasurementsBetweenAsync(filtered, filter.Start, filter.End, filter.Skip.Value,
												 filter.Limit.Value, direction).AwaitBackground();
			}

			return this.Ok(MeasurementConverter.Convert(result));
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<MQR>), StatusCodes.Status200OK)]
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

			IEnumerable<MeasurementsQueryResult> data;

			if(longitude != null && latitude != null) {
				var maxDist = radius ?? 100;
				var coords = new GeoJsonPoint {
					Latitude = latitude.Value,
					Longitude = longitude.Value
				};
					
				data = await this.m_measurements
					.GetMeasurementsNearAsync(sensor, start, end, coords, maxDist, skip, limit, orderDirection)
					.AwaitBackground();
			} else {
				data = await this.m_measurements.GetBetweenAsync(sensor, start, end,
																	 skip, limit, orderDirection).AwaitBackground();
			}

			return this.Ok(MeasurementConverter.Convert(data));
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
