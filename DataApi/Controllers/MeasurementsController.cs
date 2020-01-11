/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Storage;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class MeasurementsController : AbstractApiController 
	{
		private readonly IMeasurementCache _store;
		private readonly IMeasurementRepository _repo;
		private readonly ISensorRepository _sensors;

		public MeasurementsController(IMeasurementCache cache, IMeasurementRepository repo, ISensorRepository sensors, IHttpContextAccessor ctx) : base(ctx)
		{
			this._store = cache;
			this._repo = repo;
			this._sensors = sensors;
		}

		[HttpPost("create")]
		[ReadWriteApiKey]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Create([FromBody] RawMeasurement raw)
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

			status.ErrorCode = ReplyCode.Ok;
			status.Message = "Measurement queued!";

			await this._store.StoreAsync(raw, RequestMethod.HttpPost).AwaitBackground();
			return this.Ok(status);
		}

		[HttpGet("{sensorId}")]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Get(string sensorId, [FromQuery] DateTime start, [FromQuery] DateTime end,
			[FromQuery] int skip = -1, [FromQuery] int limit = -1)
		{
			var sensor = await this._sensors.GetAsync(sensorId).AwaitBackground();

			// TODO: check for admin
			if(sensor.Owner != this.CurrentUser.Id)
				return this.Unauthorized();

			var data = await this._repo.GetBetweenAsync(sensor, start, end, skip, limit).AwaitBackground();
			return this.Ok(data);
		}
	}
}
