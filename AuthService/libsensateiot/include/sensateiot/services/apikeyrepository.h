/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/database.h>

#include <sensateiot/models/user.h>
#include <sensateiot/models/sensor.h>

#include <sensateiot/services/abstractapikeyrepository.h>

#include <string>
#include <vector>
#include <pqxx/pqxx>
#include <boost/unordered_set.hpp>

namespace sensateiot::services
{
	class DLL_EXPORT ApiKeyRepository : public AbstractApiKeyRepository {
	public:
		explicit ApiKeyRepository() = default;
		explicit ApiKeyRepository(const config::PostgreSQL& pgsql);
		~ApiKeyRepository() override = default;

		std::vector<std::string> GetAllSensorKeys() override;
		std::vector<std::string> GetKeys(const std::vector<std::string>& ids) override;
		std::vector<std::string> GetKeysByOwners(const boost::unordered_set<boost::uuids::uuid>& ids) override;

	private:
		pqxx::connection m_connection;
	};
}
