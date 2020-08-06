/*
 * Triggers API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.NetworkApi.Controllers
{
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class TriggersController : AbstractDataController
	{
		private readonly ITriggerRepository m_triggers;
		private readonly ILogger<TriggersController> m_logger;

		public TriggersController(IHttpContextAccessor ctx, ITriggerRepository triggers,
			ISensorLinkRepository links, IApiKeyRepository keys,
			ISensorRepository sensors, ILogger<TriggersController> logger) : base(ctx, sensors, links, keys)
		{
			this.m_logger = logger;
			this.m_triggers = triggers;
		}

		private IActionResult CreateNotAuthorizedResult()
		{
			return this.Unauthorized(new Status {
				Message = "Unable to authorize current user!",
				ErrorCode = ReplyCode.NotAllowed
			});
		}

		private static Trigger CreateNumberTrigger(RawTrigger raw)
		{
			if((raw.LowerEdge != null || raw.UpperEdge != null) && raw.KeyValue == null) {
				return null;
			}

			if(string.IsNullOrEmpty(raw.KeyValue)) {
				return null;
			}

			var trigger = new Trigger {
				KeyValue = raw.KeyValue,
				UpperEdge = raw.UpperEdge,
				LowerEdge = raw.LowerEdge,
				SensorId = raw.SensorId,
			};

			return trigger;
		}

		private static Trigger CreateNaturalLanguageTrigger(RawTrigger raw)
		{
			if(string.IsNullOrEmpty(raw.FormalLanguage)) {
				return null;
			}

			var trigger = new Trigger {
				FormalLanguage = raw.FormalLanguage,
				SensorId = raw.SensorId,
				KeyValue = ""
			};

			return trigger;
		}

		[HttpPost]
		[ReadWriteApiKey]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Trigger), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromBody] RawTrigger raw)
		{
			Trigger trigger;

			var invalidInput = new Status {
				Message = "Invalid input!",
				ErrorCode = ReplyCode.BadInput
			};

			trigger = null;

			if(raw == null) {
				return this.UnprocessableEntity(invalidInput);
			}

			var type = raw.Type;

			if(string.IsNullOrEmpty(raw.SensorId) || !ObjectId.TryParse(raw.SensorId, out _)) {
				return this.UnprocessableEntity(invalidInput);
			}

			var auth = await this.AuthenticateUserForSensor(raw.SensorId).AwaitBackground();

			if(!auth) {
				return this.CreateNotAuthorizedResult();
			}

			if(type == TriggerType.Number) {
				trigger = CreateNumberTrigger(raw);
			} else if(type == TriggerType.Regex) {
				trigger = CreateNaturalLanguageTrigger(raw);
			}

			if(trigger == null) {
				invalidInput.Message = "Invalid input provided";
				return this.UnprocessableEntity(invalidInput);
			}

			trigger.Type = type;

			try {
				await this.m_triggers.CreateAsync(trigger).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to create trigger: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);
				return this.BadRequest(invalidInput);
			}

			return this.CreatedAtAction(nameof(GetById), new { triggerId = trigger.Id }, trigger);
		}

		private async Task<bool> CreatedTargetedActionAsync(TriggerAction action)
		{
			bool auth;

			if(action.Target == null)
				return false;

			if(action.Channel == TriggerActionChannel.ControlMessage) {
				var actuator = await this.m_sensors.GetAsync(action.Target).AwaitBackground();

				if(actuator == null) {
					return false;
				}

				auth = await this.AuthenticateUserForSensor(actuator, false).AwaitBackground();
			} else {
				auth = Uri.TryCreate(action.Target, UriKind.Absolute, out var result) &&
					   result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
			}

			return auth;
		}

		[HttpPost("{triggerId}/add-action")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Trigger), StatusCodes.Status201Created)]
		public async Task<IActionResult> AddAction(long triggerId, [FromBody] TriggerAction action)
		{
			if(action == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid input.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			if(action.Channel > TriggerActionChannel.MQTT && string.IsNullOrEmpty(action.Target)) {
				return this.UnprocessableEntity(new Status {
					Message = "Missing target field.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var trigger = await this.m_triggers.GetAsync(triggerId).AwaitBackground();

			action.TriggerId = triggerId;

			if(trigger == null) {
				return this.NotFound();
			}

			var authForTrigger = await this.AuthenticateUserForSensor(trigger.SensorId).AwaitBackground();

			if(action.Channel > TriggerActionChannel.MQTT) {
				authForTrigger = await this.CreatedTargetedActionAsync(action).AwaitBackground();
			}

			if(!authForTrigger) {
				return this.CreateNotAuthorizedResult();
			}

			try {
				await this.m_triggers.AddActionAsync(trigger, action).AwaitBackground();
			} catch(Exception e) {
				this.m_logger.LogInformation($"Unable to store trigger action: {e.Message}");
				this.m_logger.LogDebug(e.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to add action.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.CreatedAtAction(nameof(GetById), new { triggerId = trigger.Id }, trigger);
		}

		[HttpPost("{triggerId}/add-actions")]
		[ReadWriteApiKey]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Trigger), StatusCodes.Status201Created)]
		public async Task<IActionResult> AddActions(long triggerId, [FromBody] List<TriggerAction> actions)
		{
			if(actions == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid input.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			actions.ForEach(action => action.TriggerId = triggerId);
			var trigger = await this.m_triggers.GetAsync(triggerId).AwaitBackground();

			if(trigger == null) {
				return this.NotFound();
			}

			var auth = await this.AuthenticateUserForSensor(trigger.SensorId).AwaitBackground();

			if(!auth) {
				return this.Forbid();
			}

			try {
				await this.m_triggers.AddActionsAsync(trigger, actions).AwaitBackground();
			} catch(Exception e) {
				this.m_logger.LogInformation($"Unable to store trigger action: {e.Message}");
				this.m_logger.LogDebug(e.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to add action.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.CreatedAtAction(nameof(GetById), new { triggerId = trigger.Id }, trigger);
		}

		[HttpDelete("{triggerId}")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Delete(long triggerId)
		{
			var trigger = await this.m_triggers.GetAsync(triggerId).AwaitBackground();

			if(trigger == null) {
				return this.NotFound();
			}

			var sensor = await this.m_sensors.GetAsync(trigger.SensorId).AwaitBackground();

			if(sensor == null || sensor.Owner != this.CurrentUser.Id) {
				return this.Forbid();
			}

			await this.m_triggers.DeleteAsync(trigger.Id).AwaitBackground();
			return this.NoContent();
		}

		[HttpDelete("{triggerId}/remove-action")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> RemoveAction(long triggerId, [FromQuery] TriggerActionChannel? channel)
		{
			if(channel == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Action ID required.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var trigger = await this.m_triggers.GetAsync(triggerId).AwaitBackground();

			if(trigger == null) {
				return this.NotFound();
			}

			var sensor = await this.m_sensors.GetAsync(trigger.SensorId).AwaitBackground();

			if(sensor == null || sensor.Owner != this.CurrentUser.Id) {
				return this.Forbid();
			}

			await this.m_triggers.RemoveActionAsync(trigger, channel.Value).AwaitBackground();
			return this.NoContent();
		}

		[HttpGet("{triggerId}")]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Trigger), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetById(long triggerId)
		{
			var trigger = await this.m_triggers.GetAsync(triggerId).AwaitBackground();

			if(trigger == null) {
				return this.NotFound();
			}

			var linked = await this.IsLinkedSensor(trigger.SensorId).AwaitBackground();
			var auth = await this.AuthenticateUserForSensor(trigger.SensorId).AwaitBackground();

			if(!auth && !linked) {
				return this.CreateNotAuthorizedResult();
			}

			return this.Ok(trigger);
		}

		[HttpGet]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(IEnumerable<Trigger>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get([FromQuery] string sensorId, [FromQuery] TriggerType? type)
		{
			if(string.IsNullOrEmpty(sensorId)) {
				return this.UnprocessableEntity(new Status {
					Message = "Unknown sensor ID",
					ErrorCode = ReplyCode.BadInput
				});
			}

			if(type == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Trigger type not specified!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var linked = await this.IsLinkedSensor(sensorId).AwaitBackground();
			var auth = await this.AuthenticateUserForSensor(sensorId).AwaitBackground();

			if(!auth && !linked) {
				return this.CreateNotAuthorizedResult();
			}

			var triggers = await this.m_triggers.GetAsync(sensorId, type.Value).AwaitBackground();
			return this.Ok(triggers);
		}
	}
}