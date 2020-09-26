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

	void DataCache::Append(models::Sensor&& sensor)
	{
		auto entry = std::make_pair(sensor.GetId(), std::forward<models::Sensor>(sensor));
		this->m_sensors.AddOrUpdate(entry, {});
	}

	void DataCache::Append(models::User&& user)
	{
		auto entry = std::make_pair(user.GetId(), std::forward<models::User>(user));
		this->m_users.AddOrUpdate(entry, {});
	}

	void DataCache::Append(models::ApiKey&& key)
	{
		auto entry = std::make_pair(key.GetKey(), std::forward<models::ApiKey>(key));
		this->m_keys.AddOrUpdate(entry, {});
	}

	void DataCache::Append(std::vector<SensorPairType>&& sensors)
	{
		this->m_sensors.AddOrUpdateRange(std::forward<std::vector<SensorPairType>>(sensors), {});
	}

	void DataCache::Append(std::vector<UserPairType>&& users)
	{
		this->m_users.AddOrUpdateRange(std::forward<std::vector<UserPairType>>(users), {});
	}

	void DataCache::Append(std::vector<ApiKeyPairType>&& keys)
	{
		this->m_keys.AddOrUpdateRange(std::forward<std::vector<ApiKeyPairType>>(keys), {});
	}

	std::pair<bool, std::optional<models::Sensor>> DataCache::GetSensor(const models::ObjectId& id, TimePoint tp) const
	{
		auto sensor = this->m_sensors.TryGetValue(id, tp);

		if(!sensor.has_value()) {
			return std::make_pair(false, std::optional<models::Sensor>());
		}

		auto user = this->m_users.TryGetValue(sensor.value().GetOwner(), tp);

		if(!user.has_value()) {
			return std::make_pair(false, std::optional<models::Sensor>());
		}

		auto key = this->m_keys.TryGetValue(sensor->GetSecret(), tp);

		if(!key.has_value()) {
			return std::make_pair(false, std::optional<models::Sensor>());
		}

		auto validated = !key.value().GetReadOnly() && !key.value().GetRevoked() &&
			key.value().GetType() == models::ApiKeyType::SensorKey &&
			!user.value().GetBanned() && !user.value().GetLockout();

		if(validated) {
			return std::make_pair(true, std::move(sensor));
		}

		return std::make_pair(true, std::optional<models::Sensor>());
	}

	void DataCache::CleanupFor(std::chrono::high_resolution_clock::duration duration)
	{
		if(this->m_users.Size() > 0) {
			this->m_users.Cleanup(duration);
		}

		if(this->m_keys.Size() > 0) {
			this->m_keys.Cleanup(duration);
		}

		if(this->m_sensors.Size() > 0) {
			this->m_sensors.Cleanup(duration);
		}
	}

	void DataCache::Clear()
	{
		this->m_sensors.Clear();
		this->m_users.Clear();
		this->m_keys.Clear();
	}

	void DataCache::Cleanup()
	{
		this->m_users.Cleanup();
		this->m_keys.Cleanup();
		this->m_sensors.Cleanup();
	}

	void DataCache::FlushUser(const boost::uuids::uuid& id)
	{
		this->m_users.Remove(id);
	}

	void DataCache::FlushSensor(const models::ObjectId& id)
	{
		auto sensor = this->m_sensors.TryGetValue(id);

		if(!sensor.has_value()) {
			return;
		}

		try {
			this->m_keys.Remove(sensor.value().GetSecret());
		} catch(std::out_of_range& ) {
#ifdef DEBUG
#endif
		}
	}

	void DataCache::FlushKey(const std::string& key)
	{
		this->m_keys.Remove(key);
	}
}
