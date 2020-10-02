using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GeoJsonObjectModel;
using SensateService.Api.DataApi.Dto;
using SensateService.ApiCore.Controllers;
using SensateService.Common.Data.Dto.Generic;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Services.DataProcessing;

namespace SensateService.Api.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("data/v1/[controller]")]
	public class ExportController : AbstractDataController
	{
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorService m_sensorService;

		public ExportController(IHttpContextAccessor ctx,
								IMeasurementRepository measurements,
								ISensorService sensorService,
		                        ISensorRepository sensors,
		                        ISensorLinkRepository links,
		                        IApiKeyRepository keys) : base(ctx, sensors, links, keys)
		{
			this.m_measurements = measurements;
			this.m_sensorService = sensorService;
		}

		[HttpPost("measurements")]
		[ProducesResponseType(typeof(IEnumerable<MeasurementsQueryResult>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Filter([FromBody] Filter filter)
		{
			var measurements = await this.GetMeasurementsAsync(filter).AwaitBackground();

			if(measurements == null) {
				var status = new Status {
					Message = "Unable to fetch measurements!",
					ErrorCode = ReplyCode.BadInput
				};

				return this.UnprocessableEntity(status);
			}

			var records = new List<dynamic>();

			foreach(var measurement in measurements) {
				dynamic record = new ExpandoObject();

				foreach(var kvp in measurement.Data) {
					AddProperty(record, kvp.Key, kvp.Value.Value);
					record.Unit = kvp.Value.Unit;
				}

				record.Longitude = measurement.Location.Coordinates.Longitude;
				record.Latitude = measurement.Location.Coordinates.Latitude;

				records.Add(record);
			}

			var stream = new MemoryStream();
			await using(var writer = new StreamWriter(stream, leaveOpen: true)) {
				var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, true);
				await csv.WriteRecordsAsync(records).AwaitBackground();
			}

			stream.Position = 0;
			return this.File(stream, "application/octet-stream", "measurements.csv");
		}

		private static void AddProperty(ExpandoObject expando, string propertyName, object obj)
		{
			var expandoDict = expando as IDictionary<string, object>;

			if(expandoDict.ContainsKey(propertyName)) {
				expandoDict[propertyName] = obj;
			} else {
				expandoDict.Add(propertyName, obj);
			}
		}

		public async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsAsync(Filter filter)
		{
			var status = new Status();
			IEnumerable<MeasurementsQueryResult> result;

			if(filter.SensorIds == null || filter.SensorIds.Count <= 0) {
				status.ErrorCode = ReplyCode.BadInput;
				status.Message = "Sensor ID list cannot be empty!";

				return null;
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

				return null;
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

			return result;
		}
	}
}