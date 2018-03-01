/*
 * Sensors controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

namespace SensateService.Controllers
{
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class SensorsController : Controller
	{
		[HttpGet("{id}", Name = "GetSensor")]
		public IActionResult GetById(long id)
		{
			return new ObjectResult("{\"Hello\": \"World\"}");
		}
	}
}
