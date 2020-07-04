/*
 * Data caching header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/datacache.h>
#include <sensateiot/models/rawmeasurement.h>

namespace sensateiot::data
{
	DataCache::DataCache(long tmo) : m_sensors(tmo), m_users(tmo), m_keys(tmo), m_blackList(tmo)
	{
	}

	void DataCache::Append(std::vector<models::Sensor> &sensors)
	{
		for(auto&& sensor : sensors) {
			this->m_sensors.Emplace(sensor.GetId(), std::forward<models::Sensor>(sensor));
		}
	}

	void DataCache::Append(std::vector<models::User> &users)
	{
		for(auto&& user : users) {
			this->m_users.Emplace(user.GetId(), std::forward<models::User>(user));
		}
	}

	void DataCache::Append(std::vector<std::string>& keys)
	{
		for(auto&& key : keys) {
			this->m_keys.Emplace(std::forward<std::string>(key));
		}
	}

	void DataCache::AppendBlackList(const models::ObjectId& objId)
	{
		this->m_blackList.Insert(objId);
	}

	void DataCache::AppendBlackList(const std::vector<models::ObjectId>& objIds)
	{
		for(const auto& id : objIds) {
			this->AppendBlackList(id);
		}
	}

	std::pair<bool, std::optional<models::Sensor>> DataCache::GetSensor(const models::ObjectId& id) const
	{
		try {
			auto now = boost::chrono::high_resolution_clock::now();
			auto sensor = this->m_sensors.At(id, now);
			auto user = this->m_users.At(sensor.GetOwner(), now);
			auto validated = this->m_keys.Has(sensor.GetSecret(), now);

			validated = validated && !user.GetBanned() && !user.GetLockout();

			if(validated) {
				return std::make_pair(true, std::move(sensor));
			}

			return std::make_pair(true, std::optional<models::Sensor>());
		} catch(std::out_of_range& ) {
			/* Thrown if not found */
			return std::make_pair(false, std::optional<models::Sensor>());
		}
	}

	bool DataCache::IsBlackListed(const models::ObjectId& objId) const
	{
		return this->m_blackList.Contains(objId);
	}

	DataCache::SensorStatus DataCache::CanProcess(const models::RawMeasurement& raw) const
	{
		SensorStatus rv = SensorStatus::Unknown;
		auto now = boost::chrono::high_resolution_clock::now();

		try {
			auto sensor = this->m_sensors.At(raw.GetObjectId(), now);
			auto validKey = this->m_keys.Has(sensor.GetSecret(), now);

			if (validKey) {
				rv = SensorStatus::Available;
			} else if (this->m_blackList.Has(sensor.GetId(), now)) {
				rv = SensorStatus::Unavailable;
			}
		} catch (std::out_of_range&) {
			if (this->m_blackList.Has(raw.GetObjectId(), now)) {
				rv = SensorStatus::Unavailable;
			}
		}

		return rv;
	}

	void DataCache::CleanupFor(boost::chrono::milliseconds millis)
	{
		this->m_users.Cleanup(millis);
		this->m_keys.Cleanup(millis);
		this->m_sensors.Cleanup(millis);
		this->m_blackList.Cleanup(millis);
	}

	void DataCache::Clear()
	{
		this->m_sensors.Clear();
		this->m_users.Clear();
		this->m_keys.Clear();
		this->m_blackList.Clear();
	}

	void DataCache::Cleanup()
	{
		this->m_users.Cleanup();
		this->m_keys.Cleanup();
		this->m_sensors.Cleanup();
		this->m_blackList.Cleanup();
	}
}
