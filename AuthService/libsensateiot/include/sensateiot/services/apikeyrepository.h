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
#include <sensateiot/services/abstractpostgresqlrepository.h>

#include <optional>
#include <string>
#include <vector>
#include <pqxx/pqxx>
#include <boost/unordered_set.hpp>

namespace sensateiot::services
{
	class DLL_EXPORT ApiKeyRepository : public AbstractApiKeyRepository, public AbstractPostgresqlRepository {
	public:
		explicit ApiKeyRepository() = default;
		explicit ApiKeyRepository(const config::PostgreSQL& pgsql);
		~ApiKeyRepository() override = default;

		std::vector<models::ApiKey> GetAllSensorKeys() override;
		std::optional<models::ApiKey> GetSensorKey(const std::string& id) override;

	protected:
		void Reconnect() override;
	};
}
