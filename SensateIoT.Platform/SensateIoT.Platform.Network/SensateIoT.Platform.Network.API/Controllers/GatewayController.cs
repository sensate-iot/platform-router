/*
 * Message gateway controller.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;

namespace SensateIoT.Platform.Network.API.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class GatewayController : Controller
	{
		private readonly IMeasurementAuthorizationService m_service;

		public GatewayController(IMeasurementAuthorizationService service)
		{
			this.m_service = service;
		}

		[HttpPost("messages")]
		[ReadWriteApiKey, ValidateModel]
		public async Task<IActionResult> Messages()
		{
			await Task.CompletedTask;
			return this.NoContent();
		}

		[HttpPost("measurements")]
		[ReadWriteApiKey, ValidateModel]
		public async Task<IActionResult> Measurements()
		{
			var response = new Response<GatewayResponse>();

			using var reader = new StreamReader(this.Request.Body);
			var raw = await reader.ReadToEndAsync();

			var measurement = JsonConvert.DeserializeObject<Measurement>(raw);
			this.m_service.AddMessage(new JsonMeasurement(measurement, raw));

			response.Data = new GatewayResponse {
				Message = "Measurements received and queued.",
				Queued = 1
			};

			return this.Accepted(response);
		}
	}
}
