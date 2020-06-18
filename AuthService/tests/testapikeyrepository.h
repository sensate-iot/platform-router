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
			return std::vector<std::string>();
		}

		std::vector<std::string>
		GetKeysByOwners(const boost::unordered_set<boost::uuids::uuid> &ids) override
		{
			return std::vector<std::string>();
		}

	};
}
