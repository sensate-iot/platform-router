/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("data/v1/[controller]")]
	public class MessagesController : AbstractDataController
	{
		private readonly IMessageRepository m_messages;
		private readonly IMessageAuthorizationProxyCache m_proxy;
		private readonly ILogger<MessagesController> m_logger;

		public MessagesController(IHttpContextAccessor ctx,
								  IMessageAuthorizationProxyCache proxy,
								  ILogger<MessagesController> logger,
								  IMessageRepository messages,
								  ISensorLinkRepository links,
								  ISensorRepository sensors) : base(ctx, sensors, links)
		{
			this.m_messages = messages;
			this.m_logger = logger;
			this.m_proxy = proxy;
		}

		[HttpPost]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Message), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromQuery] bool bulk)
		{
			var status = new Status();

			try {
				using var reader = new StreamReader(this.Request.Body);
				var raw = await reader.ReadToEndAsync();

				if(!(this.HttpContext.Items["ApiKey"] is SensateApiKey)) {
					return this.Forbid();
				}

				status.ErrorCode = ReplyCode.Ok;
				status.Message = "Messages queued!";

				if(bulk) {
					this.m_proxy.AddMessages(raw);
				} else {
					this.m_proxy.AddMessage(raw);
				}

				return this.Accepted(status);
			} catch(JsonException) {
				status.Message = "Unable to parse message.";
				status.ErrorCode = ReplyCode.BadInput;
				return this.UnprocessableEntity(status);
			} catch(Exception ex) {
				status.Message = "Unable to handle request";
				status.ErrorCode = ReplyCode.UnknownError;
				this.m_logger.LogInformation($"Unable to process message: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.StatusCode(StatusCodes.Status500InternalServerError, status);
			}

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
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
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
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get([FromQuery] string sensorId, [FromQuery] DateTime? start, [FromQuery] DateTime? end,
											 [FromQuery] int skip = 0, [FromQuery] int take = 0,
											 [FromQuery] string order = "asc")
		{
			var orderDirection = order switch
			{
				"asc" => OrderDirection.Ascending,
				"desc" => OrderDirection.Descending,
				_ => OrderDirection.None,
			};

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

			var msgs = await this.m_messages.GetAsync(sensor, start.Value, end.Value, skip, take, orderDirection).AwaitBackground();
			return this.Ok(msgs);
		}
	}
}

