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
#include <sensateiot/models/apikey.h>

#include <sensateiot/data/memorycache.h>

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
		typedef std::pair<models::ObjectId, models::Sensor> SensorPairType;
		typedef std::pair<models::User::IdType, models::User> UserPairType;
		typedef std::pair<std::string, models::ApiKey> ApiKeyPairType;
		
		enum class SensorStatus {
			Available,
			Unavailable,
			Unknown
		};

		typedef std::chrono::high_resolution_clock::time_point TimePoint;
		typedef std::pair<bool, std::optional<models::Sensor>> SensorLookupType;
		static constexpr long DefaultTimeoutMinutes = 6;

		explicit DataCache(std::chrono::high_resolution_clock::duration timeout);
		explicit DataCache();

		void Append(const std::vector<models::Sensor>& sensors);
		void Append(const std::vector<models::User>& users);
		void Append(const std::vector<models::ApiKey>& keys);
		
		void Append(std::vector<SensorPairType>&& sensors);
		void Append(std::vector<UserPairType>&& users);
		void Append(std::vector<ApiKeyPairType>&& keys);

		void CleanupFor(std::chrono::high_resolution_clock::duration duration);
		void Clear();
		void Cleanup();

		void FlushUser(const boost::uuids::uuid& id);
		void FlushSensor(const models::ObjectId& id);
		void FlushKey(const std::string& key);

		/* Found, sensor data */
		std::pair<bool, std::optional<models::Sensor>> GetSensor(const models::ObjectId& id, TimePoint tp) const;

	private:
		MemoryCache<models::ObjectId, models::Sensor> m_sensors;
		MemoryCache<boost::uuids::uuid, models::User> m_users;
		MemoryCache<std::string, models::ApiKey> m_keys;
	};
}
