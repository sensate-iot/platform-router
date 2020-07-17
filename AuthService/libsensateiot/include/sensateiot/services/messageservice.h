/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/config.h>

#include <boost/uuid/uuid.hpp>
#include <boost/chrono.hpp>

#include <sensateiot/consumers/measurementconsumer.h>
#include <sensateiot/consumers/messageconsumer.h>
#include <sensateiot/mqtt/imqttclient.h>

#include <sensateiot/data/datacache.h>
#include <sensateiot/data/measurementvalidator.h>

#include <sensateiot/services/abstractuserrepository.h>
#include <sensateiot/services/abstractapikeyrepository.h>
#include <sensateiot/services/abstractsensorrepository.h>

#include <sensateiot/models/sensor.h>
#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>
#include <sensateiot/models/objectid.h>

#include <string>
#include <atomic>
#include <shared_mutex>

namespace sensateiot::services
{
	class MessageService {
		typedef std::pair<std::size_t, std::vector<models::ObjectId>> ProcessingStats;

	public:
		explicit MessageService(mqtt::IMqttClient& client,
		                        AbstractUserRepository& users,
		                        AbstractApiKeyRepository& keys,
		                        AbstractSensorRepository& sensors,
		                        const config::Config& conf);

		std::time_t Process();
		void AddMeasurement(std::string msg);
		void AddMeasurements(std::vector<std::pair<std::string, models::RawMeasurement>> measurements);

	private:
		mutable std::shared_mutex m_lock;
		config::Config m_conf;
		std::atomic_uint8_t m_index;
		std::vector<consumers::MeasurementConsumer> m_measurementHandlers;
		std::vector<consumers::MessageConsumer> m_messageHandlers;

		data::DataCache m_cache;
		data::MeasurementValidator m_validator;
		std::atomic_uint m_count;

		stl::ReferenceWrapper<AbstractApiKeyRepository> m_keyRepo;
		stl::ReferenceWrapper<AbstractUserRepository> m_userRepo;
		stl::ReferenceWrapper<AbstractSensorRepository> m_sensorRepo;

		std::vector<models::ObjectId> Process(bool postProcess);

		static constexpr int Increment = 1;
		static constexpr auto CleanupTimeout = boost::chrono::milliseconds(25);

		void Load(std::vector<models::ObjectId>& objIds);
	};
}
