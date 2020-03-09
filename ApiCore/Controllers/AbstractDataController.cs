/*
 * Abstract data API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.ApiCore.Controllers
{
	public class AbstractDataController : AbstractApiController
	{
		protected readonly ISensorRepository m_sensors;
		protected readonly ISensorLinkRepository m_links;

		public AbstractDataController(IHttpContextAccessor ctx, ISensorRepository sensors, ISensorLinkRepository links) : base(ctx)
		{
			this.m_sensors = sensors;
			this.m_links = links;
		}

		protected async Task<bool> AuthenticateUserForSensor(string sensorId, bool strict = false)
		{
			var sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();

			if(sensor == null) {
				return false;
			}

			return this.AuthenticateUserForSensor(sensor, strict);
		}

		protected async Task<bool> IsLinkedSensor(string id)
		{
			var links = await this.m_links.GetByUserAsync(this.CurrentUser).AwaitBackground();
			return links.Any(link => link.SensorId == id);
		}

		protected bool AuthenticateUserForSensor(Sensor sensor, bool strict)
		{
			var auth = sensor.Owner == this.CurrentUser.Id && this.CurrentUser.ApiKeys.Any(key => key.ApiKey == sensor.Secret);

			if(this.ApiKey == null) {
				return false;
			}

			auth = auth && !this.ApiKey.Revoked;

			if(strict) {
				auth = auth && this.ApiKey.Type == ApiKeyType.SensorKey;
				auth = auth && this.ApiKey.ApiKey == sensor.Secret;
			}

			var isHealthyUser = this.CurrentUser.UserRoles.Any(role => role.Role.Name != SensateRole.Banned);

			return auth && isHealthyUser;
		}
	}
}
