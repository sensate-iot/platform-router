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

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class SensorsController : AbstractApiController 
	{
		private readonly ISensorRepository _sensors;

		public SensorsController(IHttpContextAccessor ctx, ISensorRepository sensors) : base(ctx)
		{
			this._sensors = sensors;
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<Sensor>), 200)]
		public async Task<IActionResult> Index([FromQuery] string name)
		{
			IEnumerable<Sensor> sensors;

			if(string.IsNullOrEmpty(name))
				sensors = await this._sensors.GetAsync(this.CurrentUser).AwaitBackground();
			else
				sensors = await this._sensors.FindByNameAsync(this.CurrentUser, name).AwaitBackground();
				
			return this.Ok(sensors);
		}
	}
}
