/*
 * Data caching header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/models/sensor.h>
#include <sensateiot/models/user.h>
#include <sensateiot/models/measurement.h>

#include <sensateiot/stl/map.h>

#include <boost/unordered_set.hpp>
#include <boost/uuid/uuid.hpp>
#include <boost/chrono/chrono.hpp>

#include <vector>
#include <string>
#include <utility>
#include <optional>

namespace sensateiot::data
{
	class DataCache {
	public:
		enum class SensorStatus {
			Available,
			Unavailable,
			Unknown
		};


		typedef std::pair<bool, std::optional<models::Sensor>> SensorLookupType;
		//static constexpr long DefaultTimeout = 30 * 60 * 1000; // 30 minutes in millis
		static constexpr long DefaultTimeout = 60 * 1000; // 30 minutes in millis

		explicit DataCache(long tmo = DefaultTimeout);

		void Append(std::vector<models::Sensor>& sensors);
		void Append(std::vector<models::User>& users);
		void Append(std::vector<std::string>& keys);
		void AppendBlackList(const models::ObjectId& objId);
		void AppendBlackList(const std::vector<models::ObjectId>& objIds);

		void CleanupFor(boost::chrono::milliseconds millis);
		void Clear();
		void Cleanup();

		void FlushUser(const boost::uuids::uuid& id);
		void FlushSensor(const models::ObjectId& id);
		void FlushKey(const std::string& key);

		/* Found, sensor data */
		std::pair<bool, std::optional<models::Sensor>> GetSensor(const models::ObjectId& id) const;
		bool IsBlackListed(const models::ObjectId& objId) const;
		SensorStatus CanProcess(const models::Measurement& raw) const;

	private:
		stl::Map<models::ObjectId, models::Sensor> m_sensors;
		stl::Map<boost::uuids::uuid, models::User> m_users;
		stl::Set<std::string> m_keys;
		stl::Set<models::ObjectId> m_blackList;
	};
}
