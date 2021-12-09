using Microsoft.Extensions.Logging;
using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Exceptions;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

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
			var result = this.VerifySensor(sensor) && this.IsValidSensor(sensor, account, key);

			if(!result) {
				this.m_logger.LogInformation("Dropping message for sensor {sensorId}: authorization failed", sensor.ID.ToString());
			}

			return result;
		}

		private bool ValidateAccount(Sensor sensor, Account account, ApiKey key)
		{
			var invalid = false;

			if(account.HasBillingLockout) {
				this.m_logger.LogInformation("Skipping sensor {sensorId} due to billing lock on account {accountId:D}", sensor.ID.ToString(), account.ID);
				invalid = true;
			}

			if(account.IsBanned) {
				this.m_logger.LogInformation("Skipping sensor {sensorId} because account {accountId:D} is banned", sensor.ID.ToString(), account.ID);
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
