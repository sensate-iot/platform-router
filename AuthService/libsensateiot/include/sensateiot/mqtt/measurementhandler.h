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
#include <sensateiot/data/datacache.h>
#include <sensateiot/models/objectid.h>
#include <sensateiot/models/rawmeasurement.h>

#include <re2/re2.h>

#include <string>
#include <vector>
#include <mutex>

namespace sensateiot::mqtt
{
	class MeasurementHandler {
	public:
		explicit MeasurementHandler(IMqttClient& client, data::DataCache& cache);
		virtual ~MeasurementHandler();

		typedef std::pair<std::string, models::RawMeasurement> MeasurementPair;

		void PushMeasurement(MeasurementPair measurement);
		std::vector<models::ObjectId> Process();
		void ProcessLeftOvers();

		MeasurementHandler(MeasurementHandler&& rhs) noexcept ;
		MeasurementHandler& operator=(MeasurementHandler&& rhs) noexcept;

	private:
		stl::ReferenceWrapper<IMqttClient> m_internal;
		stl::ReferenceWrapper<data::DataCache> m_cache;
		std::vector<MeasurementPair> m_measurements;
		std::vector<MeasurementPair> m_leftOver;
		std::mutex m_lock;

		static constexpr int VectorSize = 10000;
		static re2::RE2 SearchRegex;
		typedef data::DataCache::SensorLookupType SensorLookupType;

		bool ValidateMeasurement(const models::Sensor& sensors, const MeasurementPair& pair) const;
	};
}
