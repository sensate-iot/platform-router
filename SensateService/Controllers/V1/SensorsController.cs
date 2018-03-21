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
using SensateService.Helpers;

namespace SensateService.Controllers.V1
{
	[Route("v{version:apiVersion}/[controller]")]
	[ApiVersion("1")]
	public class SensorsController : Controller
	{
		private ISensorRepository _repo;
		private readonly Random random;
		private IUserRepository _users;

		public SensorsController(ISensorRepository repository, IUserRepository users)
		{
			this._repo = repository;
			this._users = users;
			this.random = new Random();
		}

		[HttpGet("{id}", Name = "GetSensor")]
		[Authorize]
		public async Task<IActionResult> GetById(string id)
		{
			try {
				var result = await this._repo.GetAsync(id);
				if(result == null)
					return NotFound();

				return new ObjectResult(result);
			} catch(Exception ex) {
				Debug.WriteLine(ex.Message);
				return BadRequest();
			}
		}

		[HttpPost("create", Name = "CreateSensor")]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] Sensor sensor)
		{
			try {
				if(ModelState.IsValid) {
					var user = this._users.GetByClaimsPrinciple(this.User);
					if(user == null)
						return Unauthorized();

					sensor.Owner = user.Id;

					if(sensor.Secret == null)
						sensor.Secret = this.random.NextString(Sensor.SecretLength);

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
