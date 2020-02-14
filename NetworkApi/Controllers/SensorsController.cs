/*
 * Sensor HTTP controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.NetworkApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class SensorsController : AbstractDataController
	{
		private readonly ILogger<SensorsController> m_logger;
		private readonly IApiKeyRepository m_apiKeys;
		private readonly IMeasurementRepository m_measurements;
		private readonly IMessageRepository m_messages;
		private readonly ITriggerRepository m_triggers;
		private readonly ISensorStatisticsRepository m_stats;

		public SensorsController(IHttpContextAccessor ctx, ISensorRepository sensors, ILogger<SensorsController> logger,
			IMeasurementRepository measurements,
			ITriggerRepository triggers,
			IMessageRepository messages,
			ISensorStatisticsRepository stats,
			IApiKeyRepository keys) : base(ctx, sensors)
		{
			this.m_logger = logger;
			this.m_apiKeys = keys;
			this.m_measurements = measurements;
			this.m_triggers = triggers;
			this.m_messages = messages;
			this.m_stats = stats;
		}

		[HttpGet]
		[ActionName("FindSensorsByName")]
		[ProducesResponseType(typeof(IEnumerable<Sensor>), 200)]
		public async Task<IActionResult> Index([FromQuery] string name)
		{
			IEnumerable<Sensor> sensors;

			if(string.IsNullOrEmpty(name)) {
				sensors = await this.m_sensors.GetAsync(this.CurrentUser).AwaitBackground();
			} else {
				sensors = await this.m_sensors.FindByNameAsync(this.CurrentUser, name).AwaitBackground();
			}
				
			return this.Ok(sensors);
		}

		[HttpPost]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Post([FromBody] Sensor sensor)
		{
			sensor.Owner = this.CurrentUser.Id;

			try {
				await this.m_apiKeys.CreateSensorKey(new SensateApiKey(), sensor).AwaitBackground();
				await this.m_sensors.CreateAsync(sensor).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to create sensor: {ex.Message}");

				return this.BadRequest(new Status {
					Message = "Unable to save sensor.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.CreatedAtAction(nameof(Get), new {Id = sensor.InternalId}, sensor);
		}

		[HttpDelete("{id}")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> Delete(string id)
		{
			try {
				var sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

				if(sensor == null) {
					return this.NotFound();
				}

				if(!this.AuthenticateUserForSensor(sensor, false)) {
					return this.Forbid();
				}

				var tasks = new[] {
					this.m_sensors.RemoveAsync(sensor),
					this.m_apiKeys.DeleteAsync(this.CurrentUser, sensor.Secret),
					this.m_messages.DeleteBySensorAsync(sensor),
					this.m_stats.DeleteBySensorAsync(sensor),
					this.m_measurements.DeleteBySensorAsync(sensor)
				};

				await Task.WhenAll(tasks).AwaitBackground();
				await this.m_triggers.DeleteBySensorAsync(id).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to remove sensor: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to delete sensor.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.NoContent();
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(Sensor), 200)]
		public async Task<IActionResult> Get(string id)
		{
			var sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

			if(sensor == null) {
				return this.NotFound();
			}

			if(!this.AuthenticateUserForSensor(sensor, false)) {
				return this.Forbid();
			}

			return this.Ok(sensor);
		}
	}
}
