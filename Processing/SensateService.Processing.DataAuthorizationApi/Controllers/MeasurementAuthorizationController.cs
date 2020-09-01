/*
 * Measurement authorization controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
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
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization;
using SensateService.Infrastructure.Repositories;

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

		[HttpPost("measurements")]
		[ValidateModel]
		public Task<AcceptedResult> AuthorizeMeasurements([FromBody] JArray data)
		{
			var status = new Status {
				ErrorCode = ReplyCode.Ok,
				Message = "Measurements queued!"
			};

			var ary = data.Select(entry => new JsonMeasurement {
				Measurement = entry.ToObject<Measurement>(),
				Json = entry.ToString(Formatting.None)
			}).ToList();

			this.m_cache.AddMeasurements(ary);
			return Task.FromResult(this.Accepted(status));
		}

		[HttpPost("measurement")]
		public Task<AcceptedResult> AuthorizeMeasurement([FromBody] JToken data)
		{
			var status = new Status {
				ErrorCode = ReplyCode.Ok,
				Message = "Measurement queued!"
			};

			var m = new JsonMeasurement {
				Json = data.ToString(Formatting.None),
				Measurement = data.ToObject<Measurement>()
			};

			return Task.FromResult(this.Accepted(status));
		}
	}
}