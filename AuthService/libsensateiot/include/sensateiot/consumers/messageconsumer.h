/*
 * Message data consumer.
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
		MessageConsumer(MessageConsumer&& rhs) noexcept ;
		MessageConsumer& operator=(MessageConsumer&& rhs) noexcept;
		~MessageConsumer() override = default;
		
		ProcessingStats Process() override;

	private:
		typedef data::DataCache::SensorLookupType SensorLookupType;

		/* Methods */
		bool ValidateMessage(const models::Sensor& sensor, MessagePair& pair) const;
	};
}