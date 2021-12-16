using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using SensateIoT.Platform.Router.Common.Services.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;

namespace SensateIoT.Platform.Router.Service.Controllers
{
	[Produces("application/json")]
	[Route("router/v1/[controller]")]
	public class StatusController : ControllerBase
	{
		private readonly ILogger<StatusController> m_logger;
		private readonly IHealthMonitoringService m_monitoringService;

		public StatusController(IHealthMonitoringService monitor, ILogger<StatusController> logger)
		{
			this.m_logger = logger;
			this.m_monitoringService = monitor;
		}

		[HttpGet]
		public IActionResult GetStatus()
		{
			IActionResult result;

			if(this.m_monitoringService.IsHealthy) {
				result = this.NoContent();
			} else {
				result = this.StatusCode((int) HttpStatusCode.InternalServerError, this.GetErrorExplanation());
			}

			return result;
		}

		[HttpGet("reasons")]
		public IActionResult GetReasons()
		{
			var status = new GenericResponse<Status> {
				Data = new Status()
			};

			if(this.m_monitoringService.IsHealthy) {
				status.Data.IsError = false;
				status.Data.StatusText = "OK";
			} else {
				status = this.GetErrorExplanation();
			}

			return this.Ok(status);
		}

		private GenericResponse<Status> GetErrorExplanation()
		{
			var status = new GenericResponse<Status> {
				Data = new Status {
					IsError = true,
					StatusText = "FAILED"
				},
				Errors = this.m_monitoringService.GetHealthStatusExplanation()
			};

			return status;
		}
	}
}
