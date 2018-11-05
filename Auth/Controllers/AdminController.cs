/**
 * Admin control API.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Auth.Json;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.Auth.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class AdminController : AbstractController
	{
		private const int DaysPerWeek = 7;

		public AdminController(IUserRepository users) : base(users)
		{
		}

		[HttpPost("find")]
		[ProducesResponseType(typeof(List<User>), 200)]
		public async Task<IActionResult> Find([FromBody] SearchQuery query)
		{
			List<User> users;
			var result = await this._users.FindByEmailAsync(query.Query).AwaitSafely();

			users = result.Select(user => new User {
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					PhoneNumber = user.PhoneNumber,
					Id = user.Id
				}).ToList();

			return new OkObjectResult(users);
		}

		[HttpGet]
		[ProducesResponseType(typeof(AdminDashboard), 200)]
		public async Task<IActionResult> Index()
		{
			AdminDashboard db;

			db = new AdminDashboard {
				Registrations = await this.GetRegistrations()
			};

			return this.Ok(db.ToJson());
		}

		private async Task<Graph<DateTime, int>> GetRegistrations()
		{
			var now = DateTime.Now;
			Graph<DateTime, int> graph = new Graph<DateTime, int>();

			/* Include today */
			var lastweek = now.AddDays(-DaysPerWeek + 1).ToUniversalTime().Date;
			var registrations = await this._users.CountByDay(lastweek).AwaitSafely();

			for(int idx = 0; idx < DaysPerWeek; idx++) {
				var entry = registrations.ElementAtOrDefault(0);

				if(entry == null || entry.Item1 > lastweek) {
					graph.Add(lastweek, 0);
				} else {
					graph.Add(lastweek, entry.Item2);
					registrations.RemoveAt(0);
				}

				lastweek = lastweek.AddDays(1D);
			}

			graph.Data.Sort();
			return graph;
		}
	}
}