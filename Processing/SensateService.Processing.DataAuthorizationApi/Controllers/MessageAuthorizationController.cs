/*
 * Measurement authorization controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Infrastructure.Authorization;
using SensateService.Processing.DataAuthorizationApi.Dto;

namespace SensateService.Processing.DataAuthorizationApi.Controllers
{
	[Produces("application/json")]
	[Route("authorization/v1/processor")]
	public class MessageAuthorizationController : AbstractApiController
	{
		private readonly IAuthorizationCache m_cache;
		private readonly ILogger<MessageAuthorizationController> m_logger;

		public MessageAuthorizationController(IHttpContextAccessor ctx,
												  IAuthorizationCache cache,
												  ILogger<MessageAuthorizationController> logger
		                                      ) : base(ctx)
		{
			this.m_cache = cache;
			this.m_logger = logger;
		}

		[HttpPost("message")]
		[ValidateModel]
		[ProducesResponseType(typeof(AuthorizationResponse), 202)]
		[ProducesResponseType(typeof(Status), 400)]
		public Task<AcceptedResult> AuthorizeMessages([FromBody] JToken message)
		{
			var status = new AuthorizationResponse {
				ErrorCode = ReplyCode.Ok,
				Message = "Message queued!",
				Queued = 0,
				Rejected = 0
			};

			try {
				var m = new JsonMessage {
					Json = message.ToString(Formatting.None),
					Message = message.ToObject<Message>()
				};

				this.m_cache.AddMessage(m);
				status.Queued += 1;
			} catch(JsonSerializationException ex) {
				this.m_logger.LogInformation(ex, "Unable to parse message: {message}", ex.Message);
				status.Rejected += 1;
				status.Message = "Message rejected!";
			}

			return Task.FromResult(this.Accepted(status));
		}

		[HttpPost("messages")]
		[ValidateModel]
		[ProducesResponseType(typeof(AuthorizationResponse), 202)]
		[ProducesResponseType(typeof(Status), 400)]
		public Task<AcceptedResult> AuthorizeMessages([FromBody] JArray messages)
		{
			var status = new AuthorizationResponse {
				ErrorCode = ReplyCode.Ok,
				Message = "Messages queued!",
				Rejected = 0,
				Queued = 0
			};

			var ary = new List<JsonMessage>();

			foreach(var entry in messages) {
				try {
					var m = new JsonMessage {
						Message = entry.ToObject<Message>(),
						Json = entry.ToString(Formatting.None)
					};

					ary.Add(m);
					status.Queued += 1;
				} catch(JsonSerializationException ex) {
					this.m_logger.LogInformation(ex, "Unable to parse message: {message}", ex.Message);
					status.Rejected += 1;
				}
			}

			this.m_cache.AddMessages(ary);
			return Task.FromResult(this.Accepted(status));
		}

	}
}