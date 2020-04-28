/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/messageservice.h>

#include <iostream>

namespace sensateiot::mqtt
{
	MessageService::MessageService(
			IMqttClient &client,
			const services::AbstractUserRepository& users,
			const config::Config &conf
	) : m_conf(conf), m_index(0)
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
		std::unique_lock lock(this->m_lock);

		for(auto& handler : this->m_handlers) {
			handler.Process();
		}
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
}
