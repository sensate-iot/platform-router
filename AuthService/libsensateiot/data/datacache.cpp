/*
 * Data caching header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/datacache.h>



namespace sensateiot::data
{
	DataCache::DataCache(long tmo) : m_sensors(tmo), m_users(tmo), m_keys(tmo)
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

	void DataCache::CleanupFor(boost::chrono::milliseconds millis)
	{
		this->m_users.Cleanup(millis);
		this->m_keys.Cleanup(millis);
		this->m_sensors.Cleanup(millis);
	}

	std::tuple<bool, models::Sensor, bool> DataCache::GetSensor(const models::ObjectId& id) const
	{
		try {
			auto sensor = this->m_sensors.At(id);
			auto user = this->m_users.At(sensor.GetOwner());
			auto validated = this->m_keys.Has(sensor.GetSecret());

			validated = validated && !user.GetBanned() && !user.GetLockout();

			return std::make_tuple(true, std::move(sensor), validated);
		} catch(std::out_of_range& ex) {
			/* Thrown if not found */
			return std::make_tuple(false, models::Sensor(), false);
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
}
