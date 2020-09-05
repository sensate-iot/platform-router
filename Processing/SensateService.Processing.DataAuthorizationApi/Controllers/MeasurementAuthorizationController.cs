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
using SensateService.Infrastructure.Repositories;
using SensateService.Processing.DataAuthorizationApi.Dto;
using Measurement = SensateService.Common.Data.Dto.Authorization.Measurement;

namespace SensateService.Processing.DataAuthorizationApi.Controllers
{
	[Produces("application/json")]
	[Route("processor/v1")]
	public class MeasurementAuthorizationController : AbstractController
	{
		private readonly ILogger<MeasurementAuthorizationController> m_logger;
		private readonly IAuthorizationRepository m_repo;
		private readonly IAuthorizationCache m_cache;

		public MeasurementAuthorizationController(IUserRepository users,
												  ILogger<MeasurementAuthorizationController> logger,
												  IAuthorizationRepository auth,
												  IAuthorizationCache cache,
												  IHttpContextAccessor ctx) : base(users, ctx)
		{
			this.m_logger = logger;
			this.m_repo = auth;
			this.m_cache = cache;
		}

		[ValidateModel]
		[HttpPost("measurements")]
		[ProducesResponseType(typeof(AuthorizationResponse), 202)]
		[ProducesResponseType(typeof(Status), 400)]
		public Task<AcceptedResult> AuthorizeMeasurements([FromBody] JArray data)
		{
			var status = new AuthorizationResponse {
				ErrorCode = ReplyCode.Ok,
				Message = "Measurements queued!",
				Queued = 0,
				Rejected = 0
			};

			var ary = new List<JsonMeasurement>();

			foreach(var token in data) {
				try {
					var m = new JsonMeasurement {
						Json = token.ToString(Formatting.None),
						Measurement = token.ToObject<Measurement>()
					};

					ary.Add(m);
					status.Queued += 1;
				} catch(JsonSerializationException ex) {
					this.m_logger.LogInformation(ex, "Unable to parse measurement: {message}", ex.Message);
					status.Rejected += 1;
				}
			}

			this.m_cache.AddMeasurements(ary);
			return Task.FromResult(this.Accepted(status));
		}

		[ValidateModel]
		[HttpPost("measurement")]
		[ProducesResponseType(typeof(AuthorizationResponse), 202)]
		[ProducesResponseType(typeof(Status), 400)]
		public Task<AcceptedResult> AuthorizeMeasurement([FromBody] JToken data)
		{
			var status = new AuthorizationResponse {
				ErrorCode = ReplyCode.Ok,
				Message = "Measurement queued!",
				Queued = 0,
				Rejected = 0
			};

			try {
				var m = new JsonMeasurement {
					Json = data.ToString(Formatting.None),
					Measurement = data.ToObject<Measurement>()
				};

				this.m_cache.AddMeasurement(m);
				status.Queued += 1;
			} catch(JsonSerializationException ex) {
				this.m_logger.LogInformation(ex, "Unable to parse measurement: {message}", ex.Message);
				status.Rejected += 1;
				status.Message = "Measurement rejected!";
			}


			return Task.FromResult(this.Accepted(status));
		}
	}
}
