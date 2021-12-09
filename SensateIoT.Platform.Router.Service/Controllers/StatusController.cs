using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SensateIoT.Platform.Router.Service.Controllers
{
	[Produces("application/json")]
	[Route("router/v1/[controller]")]
	public class StatusController : Controller
	{
		private readonly ILogger<StatusController> m_logger;

		public StatusController(ILogger<StatusController> logger)
		{
			this.m_logger = logger;
		}
	}
}
