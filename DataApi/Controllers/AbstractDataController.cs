/*
 * Abstract data API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.DataApi.Controllers
{
	public class AbstractDataController : AbstractApiController
	{
		protected readonly ISensorRepository m_sensors;

		public AbstractDataController(IHttpContextAccessor ctx, ISensorRepository sensors) : base(ctx)
		{
			this.m_sensors = sensors;
		}

		protected async Task<bool> AuthenticateUserForSensor(string sensorId, bool strict = false)
		{
			var sensor = await this.m_sensors.GetAsync(sensorId).AwaitBackground();

			if(sensor == null) {
				return false;
			}

			var auth = sensor.Owner == this.CurrentUser.Id && this.CurrentUser.ApiKeys.Any(key => key.ApiKey == sensor.Secret);

			if(this.ApiKey == null) {
				return false;
			}

			auth = auth && !this.ApiKey.Revoked;

			if(strict) {
				auth = auth && this.ApiKey.Type == ApiKeyType.SensorKey;
				auth = auth && this.ApiKey.ApiKey == sensor.Secret;
			}

			auth = auth && this.CurrentUser.UserRoles.All(role => role.Role.Name != SensateRole.Banned);
			return auth;
		}
	}
}
