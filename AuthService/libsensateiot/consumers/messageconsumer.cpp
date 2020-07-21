
/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/consumers/messageconsumer.h>

#include <re2/re2.h>

#include <string>
#include <string_view>
#include <vector>
#include <mutex>

namespace sensateiot::consumers
{
	MessageConsumer::MessageConsumer(mqtt::IMqttClient& client, data::DataCache& cache, config::Config conf) :
		AbstractConsumer(), m_internal(client), m_cache(cache), m_config(std::move(conf))
	{
	}

	void MessageConsumer::PushMessage(MessagePair measurement)
	{
	}

	void MessageConsumer::PushMessages(std::vector<MessagePair>&& measurements)
	{
	}

	AbstractConsumer<models::Message>::ProcessingStats MessageConsumer::Process()
	{
		return {};
	}

	std::size_t MessageConsumer::PostProcess()
	{
		return 0UL;
	}
}
