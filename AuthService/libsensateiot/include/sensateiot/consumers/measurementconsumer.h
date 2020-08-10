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
#include <sensateiot/models/measurement.h>
#include <sensateiot/consumers/abstractconsumer.h>

#include <string>
#include <string_view>
#include <vector>
#include <mutex>

namespace sensateiot::consumers
{
	class MeasurementConsumer : public AbstractConsumer<models::Measurement> {
	public:
		explicit MeasurementConsumer(mqtt::IMqttClient& client, data::DataCache& cache, config::Config conf);
		MeasurementConsumer(MeasurementConsumer&& rhs) noexcept ;
		MeasurementConsumer& operator=(MeasurementConsumer&& rhs) noexcept;
		virtual ~MeasurementConsumer();

		ProcessingStats Process() override;
		std::size_t PostProcess() override;

	private:
		typedef data::DataCache::SensorLookupType SensorLookupType;
		std::vector<MessagePair> m_leftOver;

		bool ValidateMeasurement(const models::Sensor& sensor, MessagePair& pair) const;
	};
}
