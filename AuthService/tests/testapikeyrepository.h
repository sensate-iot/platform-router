/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/abstractapikeyrepository.h>

namespace sensateiot::test
{
	class DLL_EXPORT ApiKeyRepository : public services::AbstractApiKeyRepository {
	public:
		explicit ApiKeyRepository(const config::PostgreSQL& pgsql)
		{ }

		std::vector<models::ApiKey> GetAllSensorKeys() override
		{
			return std::vector<models::ApiKey>();
		}

		std::vector<models::ApiKey> GetKeys(const std::vector<std::string> &ids) override
		{
			return std::vector<models::ApiKey>();
		}

	};
}
