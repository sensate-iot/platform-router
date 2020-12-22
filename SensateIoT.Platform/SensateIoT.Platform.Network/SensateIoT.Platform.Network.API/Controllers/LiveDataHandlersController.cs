/*
 * Live data handler controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.API.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class LiveDataHandlersController : AbstractApiController
	{
		private readonly ILiveDataHandlerRepository m_repo;

		public LiveDataHandlersController(
			IHttpContextAccessor ctx,
			ILogger<LiveDataHandlersController> logger,
			ISensorLinkRepository links,
			ISensorRepository sensors,
			IApiKeyRepository keys,
			ILiveDataHandlerRepository repo) : base(ctx, sensors, links, keys)
		{
			this.m_repo = repo;
		}

		[HttpGet]
		[ValidateModel]
		[AdminApiKey]
		[ProducesResponseType(typeof(Response<IEnumerable<LiveDataHandler>>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Get()
		{
			var live = await this.m_repo.GetLiveDataHandlers().ConfigureAwait(false);

			return this.Ok(new Response<IEnumerable<LiveDataHandler>> {
				Data = live
			});
		}
	}
}
