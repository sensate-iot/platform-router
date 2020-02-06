/*
 * Sensor HTTP controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateService.ApiCore.Controllers;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.NetworkApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class SensorsController : AbstractDataController 
	{
		public SensorsController(IHttpContextAccessor ctx, ISensorRepository sensors) : base(ctx, sensors)
		{
		}

		[HttpGet]
		[ActionName("FindSensorsByName")]
		[ProducesResponseType(typeof(IEnumerable<Sensor>), 200)]
		public async Task<IActionResult> Index([FromQuery] string name)
		{
			IEnumerable<Sensor> sensors;

			if(string.IsNullOrEmpty(name)) {
				sensors = await this.m_sensors.GetAsync(this.CurrentUser).AwaitBackground();
			} else {
				sensors = await this.m_sensors.FindByNameAsync(this.CurrentUser, name).AwaitBackground();
			}
				
			return this.Ok(sensors);
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(Sensor), 200)]
		public async Task<IActionResult> Get(string id)
		{
			var sensor = await this.m_sensors.GetAsync(id).AwaitBackground();

			if(sensor == null) {
				return this.NotFound();
			}

			if(!this.AuthenticateUserForSensor(sensor, false)) {
				return this.Forbid();
			}

			return this.Ok(sensor);
		}
	}
}
