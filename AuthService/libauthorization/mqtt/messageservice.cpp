/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/messageservice.h>
#include <sensateiot/measurementhandler.h>

namespace sensateiot::mqtt
{
	MessageService::MessageService(InternalMqttClient &client, const config::Config &conf)
	{
		std::string uri = this->m_conf.GetMqtt().GetPrivateBroker().GetBroker().GetUri();

		for(int idx = 0; idx < conf.GetWorkers(); idx++) {
			MeasurementHandler handler(client);
			this->m_handlers.emplace_back(std::move(handler));
		}
	}
}
