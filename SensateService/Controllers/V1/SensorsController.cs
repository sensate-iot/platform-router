/*
 * Sensors controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Controllers.V1
{
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class SensorsController : Controller
	{
		private ISensorRepository _repo;

		public SensorsController(ISensorRepository repository)
		{
			this._repo = repository;
		}

		[HttpGet("{id}", Name = "GetSensor")]
		[Authorize]
		public async Task<IActionResult> GetById(string id)
		{
			try {
				var result = await this._repo.GetAsync(id);
				Debug.WriteLine($"Found sensor {result.InternalId.ToString()}");
				if(result == null)
					return NotFound();

				return new ObjectResult(result);
			} catch(Exception ex) {
				Debug.WriteLine(ex.Message);
				return BadRequest();
			}
		}

		[HttpPost("create", Name = "CreateSensor")]
		public async Task<IActionResult> Create([FromBody] Sensor sensor)
		{
			try {
				if(ModelState.IsValid) {
					await this._repo.CreateAsync(sensor);
					return CreatedAtRoute("GetSensor", new {Id = sensor.Secret},
						sensor);
				} else {
					return BadRequest();
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.Message);
				return BadRequest();
			}
		}
	}
}
