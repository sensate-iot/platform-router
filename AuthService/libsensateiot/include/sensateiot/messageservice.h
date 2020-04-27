/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/config.h>

#include <sensateiot/mqtt/measurementhandler.h>
#include <sensateiot/mqtt/imqttclient.h>

#include <string>
#include <atomic>
#include <shared_mutex>

#include <sensateiot/abstractuserrepository.h>

namespace sensateiot::mqtt
{
	class MessageService {
	public:
		explicit MessageService(IMqttClient& client,
				const services::AbstractUserRepository& users,
				const config::Config& conf);

		void Process();
		void AddMessage(std::string msg);

	private:
		mutable std::shared_mutex m_lock;
		config::Config m_conf;
		std::atomic_size_t m_index;
		std::vector<MeasurementHandler> m_handlers;

		static constexpr int Increment = 1;
	};
}
