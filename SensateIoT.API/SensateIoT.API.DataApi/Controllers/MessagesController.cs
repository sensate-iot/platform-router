/*
 * Measurement API controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateIoT.API.Common.ApiCore.Controllers;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("data/v1/[controller]")]
	public class MessagesController : AbstractDataController
	{
		private readonly IMessageRepository m_messages;

		public MessagesController(IHttpContextAccessor ctx,
								  IMessageRepository messages,
								  ISensorLinkRepository links,
								  IApiKeyRepository keys,
								  ISensorRepository sensors) : base(ctx, sensors, links, keys)
		{
			this.m_messages = messages;
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

			start ??= DateTime.MinValue;
			end ??= DateTime.Now;

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

		[HttpDelete]
		[ProducesResponseType(typeof(Status), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteAsync([FromQuery] string sensorId,
													 [FromQuery] DateTime? start,
													 [FromQuery] DateTime? end)
		{
			start ??= DateTime.MinValue;
			end ??= DateTime.Now;

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

			await this.m_messages.DeleteBySensorAsync(sensor, start.Value, end.Value, CancellationToken.None);
			return this.NoContent();
		}
	}
}

