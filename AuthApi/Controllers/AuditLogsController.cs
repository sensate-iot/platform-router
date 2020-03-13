/*
 * Audit log controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class AuditLogsController : AbstractController
	{
		private readonly IAuditLogRepository m_logs;
		private readonly ILogger<AuditLog> m_logger;

		public AuditLogsController(
			IUserRepository users,
			IHttpContextAccessor ctx,
			ILogger<AuditLog> logger,
			IAuditLogRepository logs
		) : base(users, ctx)
		{
			this.m_logs = logs;
			this.m_logger = logger;
		}

		private static IEnumerable<Json.AuditLog> GetJsonLogAsync(IEnumerable<AuditLog> logs, IEnumerable<SensateUser> users)
		{
			var userDict = users.ToDictionary(u => u.Id, u => u);

			return logs.Select(log => new Json.AuditLog {
				Id = log.Id,
				Timestamp = log.Timestamp,
				Email = string.IsNullOrEmpty(log.AuthorId) ? null : userDict[log.AuthorId].Email,
				Address = log.Address,
				Method = log.Method,
				Route = log.Route
			});
		}

		private async Task<int> CountAsync(SensateUser user, RequestMethod method)
		{
			int count;

			if(user == null) {
				if(method == RequestMethod.Any) {
					count = await this.m_logs.CountAsync().AwaitBackground();
				} else {
					count = await this.m_logs.CountAsync(x => x.Method == method).AwaitBackground();
				}
			} else {
				if(method == RequestMethod.Any) {
					count = await this.m_logs.CountAsync(x => x.AuthorId == user.Id).AwaitBackground();
				} else {
					count = await this.m_logs.CountAsync(x => x.Method == method && x.AuthorId == user.Id).AwaitBackground();
				}
			}

			return count;
		}

		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] int skip = 0,
											   [FromQuery] int limit = 0,
											   [FromQuery] RequestMethod method = RequestMethod.Any,
											   [FromQuery] string email = null,
											   [FromQuery] bool count = false)
		{
			IEnumerable<AuditLog> logs;
			IEnumerable<Json.AuditLog> rv = new List<Json.AuditLog>();

			try {
				SensateUser user;

				if(email != null) {
					user = await this._users.GetByEmailAsync(email).AwaitBackground();

					if(user == null) {
						return this.Ok(rv);
					}

					if(count) {
						var c = await this.CountAsync(user, method).AwaitBackground();
						return this.Ok(new {
							Count = c
						});
					}

					if(method != RequestMethod.Any) {
						logs = await this.m_logs.GetByRequestTypeAsync(user, method, skip, limit).AwaitBackground();
					} else {
						logs = await this.m_logs.GetByUserAsync(user, skip, limit).AwaitBackground();
					}

					rv = logs.Select(log => new Json.AuditLog {
						Id = log.Id,
						Email = user.Email,
						Timestamp = log.Timestamp,
						Address = log.Address,
						Method = log.Method,
						Route = log.Route
					});
				} else {
					if(count) {
						var c = await this.CountAsync(null, method).AwaitBackground();
						return this.Ok(new {
							Count = c
						});
					}

					logs = await this.m_logs.GetAllAsync(method, skip, limit).AwaitBackground();
					var list = logs.ToList();

					var ids = list.DistinctBy(x => x.AuthorId).Select(x => x.AuthorId);
					var @enum = await this._users.GetRangeAsync(ids).AwaitBackground();

					var users = @enum.ToList();
					rv = GetJsonLogAsync(list, users);
				}
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to fetch logging data: {ex.Message}!");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to fetch logging data",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.Ok(rv);
		}


		[HttpGet("find")]
		public async Task<IActionResult> Find([FromQuery] string query = null,
											  [FromQuery] int skip = 0,
											  [FromQuery] int limit = 0,
											  [FromQuery] RequestMethod method = RequestMethod.Any,
											  [FromQuery] string email = null)
		{
			IList<AuditLog> logs;
			IEnumerable<Json.AuditLog> rv;
			IList<SensateUser> users;
			var result = new PaginationResult<Json.AuditLog>();

			try {
				if(email != null) {
					users = (await this._users.FindByEmailAsync(email).AwaitBackground()).ToList();
					var ids = users.Select(x => x.Id);

					logs = (await this.m_logs.FindAsync(ids, query, method, skip, limit).AwaitBackground()).ToList();
				} else {
					logs = (await this.m_logs.FindAsync(query, method, skip, limit).AwaitBackground()).ToList();

					var ids = logs.DistinctBy(x => x.AuthorId).Select(x => x.AuthorId);
					var @enum = await this._users.GetRangeAsync(ids).AwaitBackground();

					users = @enum.ToList();
				}

				rv = GetJsonLogAsync(logs, users).ToList();
				result.Count = rv.Count();
				result.Values = rv;
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to fetch logging data: {ex.Message}!");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to fetch logging data",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.Ok(result);
		}
	}
}