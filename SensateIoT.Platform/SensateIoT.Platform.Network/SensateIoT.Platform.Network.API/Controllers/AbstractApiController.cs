/*
 * Abstract API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateIoT.Platform.Network.API.Constants;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.API.Controllers
{
	public class AbstractApiController : Controller
	{
		public User CurrentUser { get; }
		public ApiKey ApiKey { get; }

		protected string m_currentUserId;

		protected readonly ISensorRepository m_sensors;
		protected readonly ISensorLinkRepository m_links;
		protected readonly IApiKeyRepository m_keys;

		public AbstractApiController(
			IHttpContextAccessor ctx,
			ISensorRepository sensors,
			ISensorLinkRepository links,
			IApiKeyRepository keys
		)
		{
			if(ctx?.HttpContext == null) {
				return;
			}

			var key = ctx.HttpContext.Items["ApiKey"] as ApiKey;

			this.ApiKey = key;
			this.CurrentUser = key?.User;
			this.m_currentUserId = key?.UserId.ToString();
			this.m_sensors = sensors;
			this.m_links = links;
			this.m_keys = keys;
		}

		protected async Task<bool> AuthenticateUserForSensor(string sensorId, bool strict = false)
		{
			var sensor = await this.m_sensors.GetAsync(sensorId).ConfigureAwait(false);

			if(sensor == null) {
				return false;
			}

			return await this.AuthenticateUserForSensor(sensor, strict).ConfigureAwait(false);
		}

		protected async Task<bool> IsLinkedSensor(string id)
		{
			var links = await this.m_links.GetByUserAsync(this.CurrentUser.ID).ConfigureAwait(false);
			return links.Any(link => link.SensorId == id);
		}

		protected async Task<bool> AuthenticateUserForSensor(Sensor sensor, bool strict)
		{
			if(sensor == null) {
				throw new ArgumentNullException(nameof(sensor));
			}

			var sensorKey = await this.m_keys.GetAsync(sensor.Secret).ConfigureAwait(false);
			var auth = sensor.Owner == this.m_currentUserId && sensorKey != null;

			if(this.ApiKey == null) {
				return false;
			}

			auth = auth && !this.ApiKey.Revoked;

			if(strict) {
				auth = auth && this.ApiKey.Type == ApiKeyType.SensorKey;
				auth = auth && this.ApiKey.Key == sensor.Secret;
			}

			var isHealthyUser = this.CurrentUser.UserRoles.Any(role => role != UserRoles.Banned.ToUpperInvariant());

			return auth && isHealthyUser;
		}

		protected StatusCodeResult Forbidden()
		{
			return this.StatusCode(403);
		}
	}
}

