/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/services/abstractapikeyrepository.h>

namespace sensateiot::test
{
	class DLL_EXPORT ApiKeyRepository : public services::AbstractApiKeyRepository {
	public:
		explicit ApiKeyRepository(const config::PostgreSQL &pgsql)
		{
		}

		std::vector<std::string> GetAllSensorKeys() override
		{
			return std::vector<std::string>();
		}

		std::vector<std::string> GetKeys(const std::vector<std::string>& ids) override
		{
			std::vector<std::string> rv;

			for(const auto& key : this->m_keys) {
				for(auto& id : ids) {
					if(key != id) {
						continue;
					}

					rv.push_back(key);
					break;
				}
			}

			return rv;
		}
		
		std::vector<std::string> GetKeysFor(const std::vector<models::Sensor>& sensors) override
		{
			return this->m_keys;
		}

		std::vector<std::string>
		GetKeysByOwners(const boost::unordered_set<boost::uuids::uuid> &ids) override
		{
			return this->m_keys;
		}

		void AddKey(const std::string& key)
		{
			this->m_keys.push_back(key);
		}

	private:
		std::vector<std::string> m_keys;
	};
}
