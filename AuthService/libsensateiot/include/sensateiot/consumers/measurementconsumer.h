/*
 * MQTT measurement handler.
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
#include <sensateiot/models/rawmeasurement.h>
#include <sensateiot/consumers/abstractconsumer.h>

#include <re2/re2.h>

#include <string>
#include <string_view>
#include <vector>
#include <mutex>

namespace sensateiot::consumers
{
	class MeasurementConsumer : public AbstractConsumer<models::RawMeasurement> {
	public:
		explicit MeasurementConsumer(mqtt::IMqttClient& client, data::DataCache& cache, config::Config conf);
		virtual ~MeasurementConsumer();

		void PushMessage(MessagePair measurement) override;
		ProcessingStats Process() override;
		std::size_t PostProcess() override;

		MeasurementConsumer(MeasurementConsumer&& rhs) noexcept ;
		MeasurementConsumer& operator=(MeasurementConsumer&& rhs) noexcept;

	private:
		stl::ReferenceWrapper<mqtt::IMqttClient> m_internal;
		stl::ReferenceWrapper<data::DataCache> m_cache;
		std::vector<MessagePair> m_measurements;
		std::vector<MessagePair> m_leftOver;
		std::mutex m_lock;
		RE2 m_regex;
		config::Config m_config;

		typedef data::DataCache::SensorLookupType SensorLookupType;

		bool ValidateMeasurement(const models::Sensor& sensor, MessagePair& pair) const;
		void PublishAuthorizedMessages(const std::vector<models::RawMeasurement>& authorized);
	};
}
