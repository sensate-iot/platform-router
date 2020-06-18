/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/mqtt/messageservice.h>

namespace sensateiot::mqtt
{
	MessageService::MessageService(
			IMqttClient &client,
			services::AbstractUserRepository& users,
			services::AbstractApiKeyRepository& keys,
			services::AbstractSensorRepository& sensors,
			const config::Config &conf
	) : m_conf(conf), m_index(0), m_cache(), m_keyRepo(keys), m_userRepo(users), m_sensorRepo(sensors)
	{
		std::unique_lock lock(this->m_lock);
		std::string uri = this->m_conf.GetMqtt().GetPrivateBroker().GetBroker().GetUri();

		for(int idx = 0; idx < conf.GetWorkers(); idx++) {
			MeasurementHandler handler(client);
			this->m_handlers.emplace_back(std::move(handler));
		}
	}

	void MessageService::Process()
	{
		std::vector<models::ObjectId> objIds;
		std::unique_lock lock(this->m_lock);

		for(auto& handler : this->m_handlers) {
			auto result = handler.Process();
			std::move(std::begin(result), std::end(result), std::back_inserter(objIds));
		}

		std::sort(objIds.begin(), objIds.end(), [](const models::ObjectId& a, const models::ObjectId& b) {
			return a.compare(b) < 0;
		});

		auto iter = std::unique(objIds.begin(), objIds.end());
		objIds.resize( static_cast<unsigned long>(std::distance(objIds.begin(), iter)) );
		this->Load(objIds);
	}

	void MessageService::AddMessage(std::string msg)
	{
		std::shared_lock lock(this->m_lock);

		auto current = this->m_index.fetch_add(1);
		auto id = current % this->m_handlers.size();
		auto newValue = current % this->m_handlers.size();

		while(!this->m_index.compare_exchange_weak(current, newValue, std::memory_order_relaxed)) {
			newValue = current % this->m_handlers.size();
		}

		auto& repo = this->m_handlers[id];
		repo.PushMeasurement(std::move(msg));
	}

	void MessageService::ReloadAll()
	{
		this->m_cache.Clear();

		auto sensors = this->m_sensorRepo->GetAllSensors(0, 0);
		auto users   = this->m_userRepo->GetAllUsers();
		auto keys    = this->m_keyRepo->GetAllSensorKeys();

		this->m_cache.Append(sensors);
		this->m_cache.Append(users);
		this->m_cache.Append(keys);
	}

	void MessageService::Load(std::vector<models::ObjectId> ids)
	{
		auto sensors = this->m_sensorRepo->GetRange(ids, 0, 0);
		boost::unordered_set<boost::uuids::uuid> uuids;

		for(auto& sensor : sensors) {
			uuids.insert(sensor.GetOwner());
		}

		auto users = this->m_userRepo->GetRange(uuids);
		auto keys = this->m_keyRepo->GetKeysByOwners(uuids);

		this->m_cache.Append(sensors);
		this->m_cache.Append(users);
		this->m_cache.Append(keys);
	}
}
