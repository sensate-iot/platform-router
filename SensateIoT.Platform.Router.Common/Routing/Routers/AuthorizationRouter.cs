using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Exceptions;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;

namespace SensateIoT.Platform.Router.Common.Routing.Routers
{
	public class AuthorizationRouter : IRouter
	{
		public string Name => "Authorization Router";

		private readonly ILogger<AuthorizationRouter> m_logger;
		private readonly IRoutingCache m_cache;

		public AuthorizationRouter(IRoutingCache cache, ILogger<AuthorizationRouter> auth)
		{
			this.m_logger = auth;
			this.m_cache = cache;
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			var account = this.m_cache.GetAccount(sensor.AccountID);
			var key = this.m_cache.GetApiKey(sensor.SensorKey);

			return this.VerifySensor(sensor) && this.IsValidSensor(sensor, account, key);
		}

		private bool ValidateAccount(Sensor sensor, Account account, ApiKey key)
		{
			var invalid = false;

			if(account.HasBillingLockout) {
				this.m_logger.LogInformation("Skipping sensor {sensorId} due to billing lock", sensor.ID.ToString());
				invalid = true;
			}

			if(account.IsBanned) {
				this.m_logger.LogInformation("Skipping sensor because account {accountId:D} is banned", account.ID);
				invalid = true;
			}

			invalid |= account.ID != key.AccountID;
			return invalid;
		}


		private bool IsValidSensor(Sensor sensor, Account account, ApiKey key)
		{
			bool invalid;

			invalid = this.ValidateAccount(sensor, account, key);

			invalid |= key.IsReadOnly;
			invalid |= key.IsRevoked;

			return !invalid;
		}

		private bool VerifySensor(Sensor sensor)
		{
			var account = this.m_cache.GetAccount(sensor.AccountID);
			var key = this.m_cache.GetApiKey(sensor.SensorKey);

			if(account == null) {
				throw new RouterException(this.Name, $"Account with {sensor.AccountID} not found");
			}

			if(key == null) {
				throw new RouterException(this.Name, $"API key for sensor with ID {sensor.ID} not found");
			}

			return true;
		}
	}
}
