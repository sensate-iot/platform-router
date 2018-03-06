/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

using SensateService.Infrastructure.Sql;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Controllers
{
	[Route("v{version:apiVersion}/[controller]/[action]")]
	[ApiVersion("1")]
	public class AccountsController : Controller
	{
		private readonly UserAccountSettings _settings;
		private readonly SignInManager<SensateUser> _siManager;
		private readonly IUserRepository _users;

		public AccountsController(
			IUserRepository repo,
			SignInManager<SensateUser> manager,
			IOptions<UserAccountSettings> options
		)
		{
			this._users = repo;
			this._siManager = manager;
			this._settings = options.Value;
		}
	}
}
