/*
 * Actuators controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.Api.NetworkApi.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class ActuatorsController : AbstractDataController
	{
		private readonly IControlMessageRepository m_controlMessages;
		private readonly ILogger<ActuatorsController> m_logger;
		private readonly IMqttPublishService m_publisher;
		private readonly MqttPublishServiceOptions m_options;

		public ActuatorsController(IHttpContextAccessor ctx,
			IMqttPublishService publisher,
			ILogger<ActuatorsController> logger,
			ISensorRepository sensors,
			ISensorLinkRepository links,
			IApiKeyRepository keys,
			IOptions<MqttPublishServiceOptions> options,
			IControlMessageRepository msgs) : base(ctx, sensors, links, keys)
		{
			this.m_controlMessages = msgs;
			this.m_logger = logger;
			this.m_publisher = publisher;
			this.m_options = options.Value;
		}

		[HttpPost]
		[ValidateModel]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Status), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Post([FromBody] ControlMessage msg)
		{
			var auth = await this.AuthenticateUserForSensor(msg.SensorId.ToString(), true).AwaitBackground();

			if(!auth) {
				return this.Forbid();
			}

			msg.Timestamp = DateTime.UtcNow;

			try {
				var asyncio = new Task[2];
				var topic = this.m_options.ActuatorTopic.Replace("$sensorId", msg.SensorId.ToString());

				asyncio[0] = this.m_controlMessages.CreateAsync(msg);
				asyncio[1] = this.m_publisher.PublishOnAsync(topic, msg.Data, false);
				await Task.WhenAll(asyncio).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to send control message: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to send control message!",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.NoContent();
		}
	}
}
