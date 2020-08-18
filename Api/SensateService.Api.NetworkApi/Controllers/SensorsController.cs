/*
 * Sensor HTTP controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SensateService.Api.NetworkApi.Models;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Services;

namespace SensateService.Api.NetworkApi.Controllers
{
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class SensorsController : AbstractDataController
	{
		private readonly ILogger<SensorsController> m_logger;
		private readonly IUserRepository m_users;
		private readonly ISensorService m_sensorService;
		private readonly ICommandPublisher m_publish;

		public SensorsController(IHttpContextAccessor ctx,
								 ISensorRepository sensors,
								 ILogger<SensorsController> logger,
								 IUserRepository users,
								 ISensorLinkRepository links,
								 ISensorService sensorService,
								 ICommandPublisher commands,
								 IApiKeyRepository keys) : base(ctx, sensors, links, keys)
		{
			this.m_logger = logger;
			this.m_users = users;
			this.m_publish = commands;
			this.m_sensorService = sensorService;
		}

		[HttpGet]
		[ActionName("FindSensorsByName")]
		[ProducesResponseType(typeof(PaginationResult<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Index([FromQuery] string name, [FromQuery] int skip = 0, [FromQuery] int limit = 0, [FromQuery] bool link = true)
		{
			PaginationResult<Sensor> sensors;

			if(!link) {
				if(string.IsNullOrEmpty(name)) {
					var s = await this.m_sensors.GetAsync(this.CurrentUser, skip, limit).AwaitBackground();
					var count = await this.m_sensors.CountAsync(this.CurrentUser).AwaitBackground();

					sensors = new PaginationResult<Sensor> {
						Count = (int)count,
						Values = s
					};
				} else {
					var s = await this.m_sensors.FindByNameAsync(this.CurrentUser, name, skip, limit).AwaitBackground();
					var count = await this.m_sensors.CountAsync(this.CurrentUser, name).AwaitBackground();

					sensors = new PaginationResult<Sensor> {
						Count = count.ToInt(),
						Values = s
					};
				}
			} else {
				if(string.IsNullOrEmpty(name)) {
					sensors = await this.m_sensorService.GetSensorsAsync(this.CurrentUser, skip, limit).AwaitBackground();
				} else {
					sensors = await this.m_sensorService.GetSensorsAsync(this.CurrentUser, name, skip, limit).AwaitBackground();
				}
			}


			return this.Ok(sensors);
		}

		[HttpPost]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Post([FromBody] Sensor sensor)
		{
			sensor.Owner = this.CurrentUser.Id;

			try {
				await this.m_keys.CreateSensorKey(new SensateApiKey(), sensor).AwaitBackground();
				await this.m_sensors.CreateAsync(sensor).AwaitBackground();
				await this.m_publish.PublishCommand(AuthServiceCommand.FlushSensor, sensor.InternalId.ToString()).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to create sensor: {ex.Message}");

				return this.BadRequest(new Status {
					Message = "Unable to save sensor.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.CreatedAtAction(nameof(this.Get), new { Id = sensor.InternalId }, sensor);
		}

		[HttpDelete("links")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> DeleteLink([FromBody] SensorLink link)
		{
			try {
				var user = await this.m_users.GetByEmailAsync(link.UserId).AwaitBackground();
				var myemail = this.CurrentUser.Email;

				if(user == null) {
					return this.NotFound();
				}

				var auth = await this.AuthenticateUserForSensor(link.SensorId).AwaitBackground();
				auth |= myemail == link.UserId;

				if(!auth) {
					return this.Forbid();
				}

				link.UserId = user.Id;
				await this.m_links.DeleteAsync(link).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to delete link: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to remove link!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.NoContent();
		}

		[HttpGet("links")]
		[ProducesResponseType(typeof(IEnumerable<SensorLink>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> GetLinks([FromQuery][NotNull] string sensorId)
		{
			IEnumerable<SensorLink> links;

			try {
				var sensorTask = this.m_sensors.GetAsync(sensorId);

				links = await this.m_links.GetAsync(sensorId);

				var sensor = await sensorTask.AwaitBackground();

				if(sensor == null) {
					return this.NotFound();
				}

				var linkList = links.ToList();
				var users = await this.m_users.GetRangeAsync(linkList.Select(x => x.UserId)).AwaitBackground();
				var dict = users.ToDictionary(x => x.Id, x => x);

				if(!string.IsNullOrEmpty(sensorId) && this.CurrentUser.Id != sensor.Owner) {
					linkList.RemoveAll(x => x.UserId != this.CurrentUser.Id);
				}

				foreach(var link in linkList) {
					if(!dict.TryGetValue(link.UserId, out var user)) {
						continue;
					}

					link.UserId = user.Email;
				}

				return this.Ok(linkList);
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable get links: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable get links!",
					ErrorCode = ReplyCode.BadInput
				});
			}
		}

		[HttpPost("links")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Link([FromBody] SensorLink link)
		{
			var status = new Status();

			if(link.UserId == this.CurrentUser.Email) {
				status.Message = "Unable to link sensor to own account!";
				status.ErrorCode = ReplyCode.BadInput;
				return this.BadRequest(status);
			}

			try {
				var sensor = await this.m_sensors.GetAsync(link.SensorId).AwaitBackground();
				var user = await this.m_users.GetByEmailAsync(link.UserId).AwaitBackground();

				if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground()) {
					return this.Forbid();
				}

				if(user == null) {
					status.Message = "Target user not known!";
					status.ErrorCode = ReplyCode.BadInput;

					return this.BadRequest(status);
				}

				if(!user.EmailConfirmed) {
					status.Message = "Target user must have a confirmed account!";
					status.ErrorCode = ReplyCode.BadInput;

					return this.BadRequest(status);
				}

				if(sensor == null) {
					status.Message = "Target sensor not known!";
					status.ErrorCode = ReplyCode.BadInput;

					return this.BadRequest(status);
				}

				link.UserId = user.Id;
				await this.m_links.CreateAsync(link).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to link sensor: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to link sensor!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.NoContent();
		}

		[HttpPatch("{id}")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Patch(string id, [FromBody] SensorUpdate update)
		{
			Sensor sensor;
			if(!ObjectId.TryParse(id, out _)) {
				return this.UnprocessableEntity(new Status {
					Message = "Unable to parse ID",
					ErrorCode = ReplyCode.BadInput
				});
			}


			try {
				sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

				if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground()) {
					return this.Forbid();
				}

				if(sensor == null) {
					return this.NotFound();
				}

				if(!string.IsNullOrEmpty(update.Name)) {
					sensor.Name = update.Name;
				}

				if(!string.IsNullOrEmpty(update.Description)) {
					sensor.Description = update.Description;
				}

				await this.m_sensors.UpdateAsync(sensor).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to update sensor: {ex.Message}");

				return this.BadRequest(new Status {
					Message = "Unable to update sensor.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.Ok(sensor);
		}

		[HttpPatch("{id}/secret")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> PatchSecret(string id, [FromBody] SensorUpdate update)
		{
			Sensor sensor;

			update ??= new SensorUpdate();
			if(!ObjectId.TryParse(id, out _)) {
				return this.UnprocessableEntity(new Status {
					Message = "Unable to parse ID",
					ErrorCode = ReplyCode.BadInput
				});
			}

			try {
				sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

				if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground()) {
					return this.Forbid();
				}

				if(sensor == null) {
					return this.NotFound();
				}

				var key = await this.m_keys.GetByKeyAsync(update.Secret).AwaitBackground();
				var old = await this.m_keys.GetByKeyAsync(sensor.Secret).AwaitBackground();

				if(key != null) {
					return this.BadRequest(new Status {
						Message = "Unable to update sensor.",
						ErrorCode = ReplyCode.BadInput
					});
				}

				var oldSecret = sensor.Secret;
				sensor.Secret = string.IsNullOrEmpty(update.Secret) ? this.m_keys.GenerateApiKey() : update.Secret;

				if(!string.IsNullOrEmpty(update.Name)) {
					sensor.Name = update.Name;
				}

				if(!string.IsNullOrEmpty(update.Description)) {
					sensor.Description = update.Description;
				}

				await Task.WhenAll(
					this.m_keys.RefreshAsync(old, sensor.Secret),
					this.m_sensors.UpdateSecretAsync(sensor, old)
					).AwaitBackground();
				await this.m_sensors.UpdateAsync(sensor).AwaitBackground();

				await Task.WhenAll(
					this.m_publish.PublishCommand(AuthServiceCommand.FlushSensor, sensor.InternalId.ToString()),
					this.m_publish.PublishCommand(AuthServiceCommand.FlushKey, sensor.Secret),
					this.m_publish.PublishCommand(AuthServiceCommand.FlushKey, oldSecret)
				).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to update sensor: {ex.Message}");

				return this.BadRequest(new Status {
					Message = "Unable to update sensor.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.Ok(sensor);
		}

		[HttpDelete("{id}")]
		[ReadWriteApiKey]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Delete(string id)
		{
			try {
				var sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

				if(sensor == null) {
					return this.NotFound();
				}

				if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground()) {
					return this.Forbid();
				}

				await this.m_sensorService.DeleteAsync(sensor, CancellationToken.None).AwaitBackground();
				await Task.WhenAll(
					this.m_publish.PublishCommand(AuthServiceCommand.FlushSensor, sensor.InternalId.ToString()),
					this.m_publish.PublishCommand(AuthServiceCommand.FlushKey, sensor.Secret)
				).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to remove sensor: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to delete sensor.",
					ErrorCode = ReplyCode.UnknownError
				});
			}

			return this.NoContent();
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Get(string id)
		{
			var sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

			if(sensor == null) {
				return this.NotFound();
			}

			var linked = await this.IsLinkedSensor(id).AwaitBackground();

			if(!await this.AuthenticateUserForSensor(sensor, false).AwaitBackground() && !linked) {
				return this.Forbid();
			}

			return this.Ok(sensor);
		}
	}
}
