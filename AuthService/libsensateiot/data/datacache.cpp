/*
 * Data caching header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/datacache.h>
#include <sensateiot/models/measurement.h>

namespace sensateiot::data
{
	DataCache::DataCache(std::chrono::high_resolution_clock::duration timeout) : m_sensors(timeout), m_users(timeout), m_keys(timeout)
	{
	}

	DataCache::DataCache() :
		m_sensors(std::chrono::minutes(DefaultTimeoutMinutes)),
		m_users(std::chrono::minutes(DefaultTimeoutMinutes)),
		m_keys(std::chrono::minutes(DefaultTimeoutMinutes))
	{
	}

	void DataCache::Append(std::vector<models::Sensor> &sensors)
	{
		for(auto&& sensor : sensors) {
			auto entry = std::make_pair(sensor.GetId(), std::forward<models::Sensor>(sensor));
			this->m_sensors.Add(std::move(entry), {});
		}
	}

	void DataCache::Append(std::vector<models::User> &users)
	{
		for(auto&& user : users) {
			auto entry = std::make_pair(user.GetId(), std::forward<models::User>(user));
			this->m_users.Add(std::move(entry), {});
		}
	}

	void DataCache::Append(std::vector<models::ApiKey>& keys)
	{
		for(auto&& key : keys) {
			//this->m_keys.Emplace(std::forward<std::string>(key));
		}
	}

	void DataCache::AppendBlackList(const models::ObjectId& objId)
	{
	}

	void DataCache::AppendBlackList(const std::vector<models::ObjectId>& objIds)
	{
		for(const auto& id : objIds) {
			this->AppendBlackList(id);
		}
	}

	std::pair<bool, std::optional<models::Sensor>> DataCache::GetSensor(const models::ObjectId& id, TimePoint tp) const
	{
		try {
			/*auto sensor = this->m_sensors.At(id, tp);
			auto user = this->m_users.At(sensor.GetOwner(), tp);
			auto validated = this->m_keys.Has(sensor.GetSecret(), tp);

			validated = validated && !user.GetBanned() && !user.GetLockout();

			if(validated) {
				return std::make_pair(true, std::move(sensor));
			}

			return std::make_pair(true, std::optional<models::Sensor>());*/
		} catch(std::out_of_range& ) {
			/* Thrown if not found */
			return std::make_pair(false, std::optional<models::Sensor>());
		}

		return {};
	}

	bool DataCache::IsBlackListed(const models::ObjectId& objId) const
	{
		return false;
		//return this->m_blackList.Contains(objId);
	}

	DataCache::SensorStatus DataCache::CanProcess(const models::Measurement& raw) const
	{
		SensorStatus rv = SensorStatus::Unknown;
		auto now = boost::chrono::high_resolution_clock::now();

		try {
			/*auto sensor = this->m_sensors.At(raw.GetObjectId(), now);
			auto validKey = this->m_keys.Has(sensor.GetSecret(), now);

			if (validKey) {
				rv = SensorStatus::Available;
			} else if (this->m_blackList.Has(sensor.GetId(), now)) {
				rv = SensorStatus::Unavailable;
			}*/
		} catch (std::out_of_range&) {
			rv = SensorStatus::Unavailable;
		}

		return rv;
	}

	void DataCache::CleanupFor(boost::chrono::milliseconds millis)
	{
		/*if(this->m_users.Size() > 0) {
			this->m_users.Cleanup(millis);
		}

		if(this->m_keys.Size() > 0) {
			this->m_keys.Cleanup(millis);
		}

		if(this->m_sensors.Size() > 0) {
			this->m_sensors.Cleanup(millis);
		}

		if(this->m_blackList.Size() > 0) {
			this->m_blackList.Cleanup(millis);
		}*/
	}

	void DataCache::Clear()
	{
		this->m_sensors.Clear();
		//this->m_users.Clear();
		this->m_keys.Clear();
	}

	void DataCache::Cleanup()
	{
		//this->m_users.Cleanup();
		this->m_keys.Cleanup();
		this->m_sensors.Cleanup();
	}

	void DataCache::FlushUser(const boost::uuids::uuid& id)
	{
		//this->m_users.Remove(id);
	}

	void DataCache::FlushSensor(const models::ObjectId& id)
	{
		/*try {
			this->m_sensors.Process(id, [this](const models::Sensor& sensor) {
				const auto& secret = sensor.GetSecret();
				this->FlushKey(secret);
			});

			this->m_blackList.Erase(id);
			this->m_sensors.Erase(id);
		} catch(std::out_of_range&) {
			this->m_blackList.Erase(id);
			this->m_sensors.Erase(id);
		}*/
	}

	void DataCache::FlushKey(const std::string& key)
	{
		this->m_keys.Remove(key);
	}
}
