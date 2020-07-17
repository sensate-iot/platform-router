/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <config/config.h>

#include <sensateiot/mqtt/imqttclient.h>
#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/data/datacache.h>
#include <sensateiot/models/objectid.h>
#include <sensateiot/models/message.h>
#include <sensateiot/consumers/abstractconsumer.h>

#include <re2/re2.h>

#include <string>
#include <string_view>
#include <vector>
#include <mutex>

namespace sensateiot::consumers
{
	class MessageConsumer : public AbstractConsumer<models::Message> {
	public:
		explicit MessageConsumer(mqtt::IMqttClient& client, data::DataCache& cache, config::Config conf);
		
		void PushMessage(MessagePair measurement) override;
		void PushMessages(std::vector<MessagePair>&& measurements) override;
		ProcessingStats Process() override;
		std::size_t PostProcess() override;

	private:
		stl::ReferenceWrapper<mqtt::IMqttClient> m_internal;
		stl::ReferenceWrapper<data::DataCache> m_cache;
		config::Config m_config;
	};
}
