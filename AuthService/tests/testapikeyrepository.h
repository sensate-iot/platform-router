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

		std::vector<models::ApiKey> GetAllSensorKeys() override
		{
			return std::vector<models::ApiKey>();
		}
		
		std::optional<models::ApiKey> GetSensorKey(const std::string& id) override
		{
			for (auto key : this->m_keys) {
				if(key.GetKey() == id) {
					return key;
				}
			}

			return {};
		}

		void AddKey(const models::ApiKey& key)
		{
			this->m_keys.push_back(key);
		}

	private:
		std::vector<models::ApiKey> m_keys;
	};
}
