/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <config/mqtt.h>

#include <sensateiot/mqtt/imqttclient.h>
#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/models/objectid.h>

#include <string>
#include <vector>
#include <mutex>

namespace sensateiot::mqtt
{
	class MeasurementHandler {
	public:
		explicit MeasurementHandler(IMqttClient& client);
		virtual ~MeasurementHandler();

		void PushMeasurement(std::string json);
		std::vector<models::ObjectId> Process();

		MeasurementHandler(MeasurementHandler&& rhs) noexcept ;
		MeasurementHandler& operator=(MeasurementHandler&& rhs) noexcept;

	private:
		stl::ReferenceWrapper<IMqttClient> m_internal;
		std::vector<std::string> m_measurements;
		std::mutex m_lock;

		static constexpr int VectorSize = 10000;
	};
}
