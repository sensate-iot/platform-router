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
using SensateIoT.API.Common.ApiCore.Attributes;
using SensateIoT.API.Common.ApiCore.Controllers;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateService.Api.AuthApi.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("auth/v1/[controller]")]
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
			}).ToList();
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationResult<Json.AuditLog>), 200)]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(401)]
		[ProducesResponseType(403)]
		public async Task<IActionResult> Index([FromQuery] int skip = 0,
											   [FromQuery] int limit = 0,
											   [FromQuery] RequestMethod method = RequestMethod.Any,
											   [FromQuery] string email = null,
											   [FromQuery] string order = "")
		{
			PaginationResult<AuditLog> logs;
			var rv = new PaginationResult<Json.AuditLog>();


			var orderDirection = order switch
			{
				"asc" => OrderDirection.Ascending,
				"desc" => OrderDirection.Descending,
				_ => OrderDirection.None,
			};

			try {
				SensateUser user;

				if(email != null) {
					user = await this._users.GetByEmailAsync(email).AwaitBackground();

					if(user == null) {
						return this.Ok(rv);
					}

					if(method != RequestMethod.Any) {
						logs = await this.m_logs.GetByRequestTypeAsync(user, method, skip, limit, orderDirection).AwaitBackground();
					} else {
						logs = await this.m_logs.GetByUserAsync(user, skip, limit, orderDirection).AwaitBackground();
					}

					rv.Values = logs.Values.Select(log => new Json.AuditLog {
						Id = log.Id,
						Email = user.Email,
						Timestamp = log.Timestamp,
						Address = log.Address,
						Method = log.Method,
						Route = log.Route
					});
					rv.Count = logs.Count;
				} else {
					logs = await this.m_logs.GetAllAsync(method, skip, limit, orderDirection).AwaitBackground();

					var ids = logs.Values.DistinctBy(x => x.AuthorId).Select(x => x.AuthorId);
					var @enum = await this._users.GetRangeAsync(ids).AwaitBackground();
					var users = @enum.ToList();

					rv.Values = GetJsonLogAsync(logs.Values, users);
					rv.Count = logs.Count;
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
		[ProducesResponseType(typeof(PaginationResult<Json.AuditLog>), 200)]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(401)]
		[ProducesResponseType(403)]
		public async Task<IActionResult> Find([FromQuery] string query = null,
											  [FromQuery] int skip = 0,
											  [FromQuery] int limit = 0,
											  [FromQuery] RequestMethod method = RequestMethod.Any,
											  [FromQuery] string email = null,
											  [FromQuery] string order = "")
		{
			IList<SensateUser> users;
			var result = new PaginationResult<Json.AuditLog>();
			PaginationResult<AuditLog> logs;

			var orderDirection = order switch
			{
				"asc" => OrderDirection.Ascending,
				"desc" => OrderDirection.Descending,
				_ => OrderDirection.None,
			};

			try {
				if(email != null) {
					users = (await this._users.FindByEmailAsync(email).AwaitBackground()).ToList();
					var ids = users.Select(x => x.Id);

					logs = await this.m_logs.FindAsync(ids, query, method, skip, limit, orderDirection).AwaitBackground();
				} else {
					logs = await this.m_logs.FindAsync(query, method, skip, limit, orderDirection).AwaitBackground();

					var ids = logs.Values.DistinctBy(x => x.AuthorId).Select(x => x.AuthorId);
					var @enum = await this._users.GetRangeAsync(ids).AwaitBackground();

					users = @enum.ToList();
				}

				result.Values = GetJsonLogAsync(logs.Values, users).ToList();
				result.Count = logs.Count;
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