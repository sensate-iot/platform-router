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
	[Route("[controller]")]
	public class TriggersController : AbstractDataController
	{
		private readonly ITriggerRepository m_triggers;
		private readonly ILogger<TriggersController> m_logger;

		public TriggersController(IHttpContextAccessor ctx, ITriggerRepository triggers, ISensorRepository sensors, ILogger<TriggersController> logger) :
			base(ctx, sensors)
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

		[HttpPost]
		[ReadWriteApiKey]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromBody] RawTrigger raw)
		{
			if(raw == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid input!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var trigger = new Trigger {
				KeyValue = raw.KeyValue,
				UpperEdge = raw.UpperEdge,
				LowerEdge = raw.LowerEdge,
				SensorId = raw.SensorId,
				Message = raw.Message
			};

			if(string.IsNullOrEmpty(raw.KeyValue)) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid key value!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			if(string.IsNullOrEmpty(raw.SensorId) || !ObjectId.TryParse(raw.SensorId, out _)) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid sensor ID!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var auth = await this.AuthenticateUserForSensor(raw.SensorId).AwaitBackground();

			if(!auth) {
				return this.CreateNotAuthorizedResult();
			}

			await this.m_triggers.CreateAsync(trigger).AwaitBackground();

			return this.CreatedAtAction(nameof(GetById), new {triggerId = trigger.Id}, trigger);
		}

		private async Task<bool> CreatedTargetedActionAsync(TriggerAction action)
		{
			bool auth;

			if(action.Channel == TriggerActionChannel.ControlMessage) {
				var actuator = await this.m_sensors.GetAsync(action.Target).AwaitBackground(); 

				if(actuator == null) {
					return false;
				}

				auth = this.AuthenticateUserForSensor(actuator, false);
			} else {
				auth = Uri.TryCreate(action.Target, UriKind.Absolute, out var result) &&
				       result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
			}

			return auth;
		}

		[HttpPost("{triggerId}/add-action")]
		[ReadWriteApiKey]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
		public async Task<IActionResult> AddAction(long triggerId, [FromBody] TriggerAction action)
		{
			if(action == null) {
				return this.UnprocessableEntity(new Status {
					Message = "Invalid input.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			if(action.Channel > TriggerActionChannel.MQTT && string.IsNullOrEmpty( action.Target)) {
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

			return this.CreatedAtAction(nameof(GetById), new {triggerId = trigger.Id}, trigger);
		}

		[HttpPost("{triggerId}/add-actions")]
		[ReadWriteApiKey]
		[ValidateModel]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
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

			return this.CreatedAtAction(nameof(GetById), new {triggerId = trigger.Id}, trigger);
		}

		[HttpGet("{triggerId}")]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
		public async Task<IActionResult> GetById(long triggerId)
		{
			var trigger = await this.m_triggers.GetAsync(triggerId).AwaitBackground();

			if(trigger == null) {
				return this.NotFound();
			}

			var auth = await this.AuthenticateUserForSensor(trigger.SensorId).AwaitBackground();

			if(!auth) {
				return this.CreateNotAuthorizedResult();
			}

			return this.Ok(trigger);
		}

		[HttpGet]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
		public async Task<IActionResult> Get([FromQuery] string sensorId)
		{
			if(string.IsNullOrEmpty(sensorId)) {
				return this.UnprocessableEntity(new Status {
					Message = "Unknown sensor ID",
					ErrorCode = ReplyCode.BadInput
				});
			}

			var auth = await this.AuthenticateUserForSensor(sensorId).AwaitBackground();

			if(!auth) {
				return this.CreateNotAuthorizedResult();
			}

			var triggers = await this.m_triggers.GetAsync(sensorId).AwaitBackground();
			return this.Ok(triggers);
		}
	}
}