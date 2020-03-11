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

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class MessagesController : AbstractDataController
	{
		private readonly IMessageRepository m_messages;
		private readonly ILogger<MessagesController> m_logger;

		public MessagesController(IHttpContextAccessor ctx, IMessageRepository messages,
			ISensorLinkRepository links,
			ILogger<MessagesController> logger, ISensorRepository sensors) : base(ctx, sensors, links)
		{
			this.m_messages = messages;
			this.m_logger = logger;
		}

		[HttpPost]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromBody] RawMessage raw)
		{
			var msg = new Message {
				UpdatedAt = DateTime.Now,
				CreatedAt = raw.CreatedAt ?? DateTime.Now,
				Data = raw.Data
			};

			if(!ObjectId.TryParse(raw.SensorId, out var tmp)) {
				return this.UnprocessableEntity(new Status {
					ErrorCode = ReplyCode.BadInput,
					Message = "Invalid sensor ID"
				});
			}

			msg.SensorId = tmp;
			msg.InternalId = ObjectId.GenerateNewId(msg.CreatedAt);
			var auth = await this.AuthenticateUserForSensor(raw.SensorId, true).AwaitBackground();

			if(!auth) {
				return this.Unauthorized(new Status {
					Message = "Unable to authorize current user!",
					ErrorCode = ReplyCode.NotAllowed
				});
			}

			try {
				await this.m_messages.CreateAsync(msg).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation("Unable to store message: " + ex.Message);
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to store message.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.CreatedAtAction(nameof(Get), new { messageId = msg.InternalId }, msg);
		}

		private IActionResult CreateNotAuthorizedResult()
		{
			return this.Unauthorized(new Status {
				Message = "Unable to authorize current user!",
				ErrorCode = ReplyCode.NotAllowed
			});
		}

		[HttpGet("{messageId}")]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get(string messageId)
		{
			var msg = await this.m_messages.GetAsync(messageId).AwaitBackground();

			if(msg == null) {
				return this.NotFound();
			}

			var auth = await this.AuthenticateUserForSensor(msg.SensorId.ToString()).AwaitBackground();

			return auth ? this.Ok(msg) : this.CreateNotAuthorizedResult();
		}

		[HttpGet]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get([FromQuery] string sensorId, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] int skip = 0, [FromQuery] int take = -1)
		{
			if(start == null) {
				start = DateTime.MinValue;
			}

			if(end == null) {
				end = DateTime.Now;
			}

			start = start.Value.ToUniversalTime();
			end = end.Value.ToUniversalTime();

			var auth = await this.AuthenticateUserForSensor(sensorId).AwaitBackground();

			if(!auth) {
				return this.CreateNotAuthorizedResult();
			}

			var sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();

			if(sensor == null) {
				return this.NotFound();
			}

			var msgs = await this.m_messages.GetAsync(sensor, start.Value, end.Value, skip, take).AwaitBackground();
			return this.Ok(msgs);
		}
	}
}

