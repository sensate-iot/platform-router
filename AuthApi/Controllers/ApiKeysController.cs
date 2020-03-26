/*
 * API key controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
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
using SensateService.AuthApi.Json;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
{
	[Produces("application/json")]
	[Route("auth/v1/[controller]")]
	[NormalUser]
	public class ApiKeysController : AbstractController
	{
		private readonly IApiKeyRepository _keys;
		private readonly ILogger<ApiKeysController> m_logger;

		public ApiKeysController(IUserRepository users, IApiKeyRepository keys,
								 ILogger<ApiKeysController> logger,
		                         IHttpContextAccessor ctx) : base(users, ctx)
		{
			this._keys = keys;
			this.m_logger = logger;
		}

		[HttpPost("create")]
		[ActionName("CreateApiKey")]
		public async Task<IActionResult> Create([FromBody] CreateApiKey request)
		{
			var key = new SensateApiKey {
				Id = Guid.NewGuid().ToString(),
				UserId = this.CurrentUser.Id,
				CreatedOn = DateTime.Now.ToUniversalTime(),
				Revoked = false,
				Type = ApiKeyType.ApiKey,
				User = this.CurrentUser,
				ReadOnly = request.ReadOnly,
				Name = request.Name
			};

			await this._keys.CreateAsync(key).AwaitBackground();
			return this.CreatedAtAction("CreateApiKey", key);
		}

		private async Task<IActionResult> RevokeAll(bool systemonly)
		{
			var keys = await this._keys.GetByUserAsync(this.CurrentUser).AwaitBackground();
			IEnumerable<SensateApiKey> sorted;

			sorted = systemonly ? keys.Where(key => key.Revoked == false && key.Type == ApiKeyType.SystemKey) :
				keys.Where(key => key.Revoked == false && (key.Type == ApiKeyType.SystemKey || key.Type == ApiKeyType.ApiKey));

			await this._keys.MarkRevokedRangeAsync(sorted).AwaitBackground();
			return this.Ok();
		}

		[HttpDelete("revoke")]
		public async Task<IActionResult> Revoke([FromQuery] string id, [FromQuery] string key, [FromQuery] bool system = true)
		{
			SensateApiKey apikey;

			if(string.IsNullOrEmpty(id) && string.IsNullOrEmpty(key))
				return await this.RevokeAll(system).AwaitBackground();

			if(id != null) {
				apikey = await this._keys.GetByIdAsync(id).AwaitBackground();
			} else {
				apikey = await this._keys.GetByKeyAsync(key).AwaitBackground();
			}

			if(apikey == null) {
				return this.BadRequest();
			}

			if(apikey.Revoked) {
				return this.BadRequest();
			}

			if(apikey.UserId != this.CurrentUser.Id ||
			   !(apikey.Type == ApiKeyType.ApiKey || apikey.Type == ApiKeyType.SystemKey || apikey.Type == ApiKeyType.SensorKey)) {
				return this.BadRequest();
			}

			await this._keys.MarkRevokedAsync(apikey).AwaitBackground();
			return this.Ok();
		}

		[HttpPatch("{key}")]
		[ActionName("RefreshApiKey")]
		public async Task<IActionResult> Refresh(string key)
		{
			var apikey = await this._keys.GetByIdAsync(key).AwaitBackground();

			if(apikey == null) {
				return this.NotFound();
			}

			if(!(apikey.Type == ApiKeyType.ApiKey || apikey.Type == ApiKeyType.SystemKey)) {
				return this.BadRequest();
			}

			apikey = await this._keys.RefreshAsync(apikey).AwaitBackground();
			return this.CreatedAtAction("RefreshApiKey", apikey);
		}

		[HttpPost]
		public async Task<IActionResult> Filter([FromBody] ApiKeyFilter filter)
		{
			PaginationResult<SensateApiKey> keys;

			if(filter == null) {
				filter = new ApiKeyFilter() {
					Limit = 0,
					Skip = 0,
					IncludeRevoked = true
				};
			}

			if(filter.Limit == null) {
				filter.Limit = 0;
			}

			if(filter.Skip == null) {
				filter.Skip = 0;
			}

			if(filter.Types == null || filter.Types.Count <= 0) {
				filter.Types = new List<ApiKeyType> {
					ApiKeyType.ApiKey,
					ApiKeyType.SensorKey,
					ApiKeyType.SystemKey
				};
			}

			try {
				keys = await this._keys.FilterAsync(this.CurrentUser, filter.Types, filter.Query, filter.IncludeRevoked,
				                              filter.Skip.Value, filter.Limit.Value).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation(ex, "Failed to fetch keys!");

				return this.BadRequest(new Status {
					Message = "Unable to fetch API keys",
					ErrorCode = ReplyCode.UnknownError
				});
			}

			return this.Ok(keys);
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var keys = await this._keys.GetByUserAsync(this.CurrentUser).AwaitBackground();
			return this.Ok(keys);
		}
	}
}
