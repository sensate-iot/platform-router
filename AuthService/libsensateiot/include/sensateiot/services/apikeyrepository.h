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
#include <sensateiot/models/apikey.h>
#include <sensateiot/models/sensor.h>

#include <sensateiot/services/abstractapikeyrepository.h>

#include <string>
#include <vector>
#include <pqxx/pqxx>

namespace sensateiot::services
{
	class DLL_EXPORT ApiKeyRepository : public AbstractApiKeyRepository {
	public:
		explicit ApiKeyRepository() = default;
		explicit ApiKeyRepository(config::PostgreSQL pgsql);
		~ApiKeyRepository() override = default;

		std::vector<models::ApiKey> GetAllSensorKeys() override;
		std::vector<models::ApiKey> GetKeys(const std::vector<std::string>& ids) override;

	private:
		pqxx::connection m_connection;
	};
}
