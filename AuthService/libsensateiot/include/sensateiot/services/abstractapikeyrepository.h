/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/database.h>

#include <sensateiot/services/abstractpostgresqlrepository.h>
#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>
#include <sensateiot/models/sensor.h>

#include <string>
#include <vector>
#include <boost/unordered_set.hpp>


namespace sensateiot::services
{
	class AbstractApiKeyRepository {
	public:
		explicit AbstractApiKeyRepository() = default;
		explicit AbstractApiKeyRepository(config::PostgreSQL pgsql);
		virtual ~AbstractApiKeyRepository() = default;

		virtual std::vector<models::ApiKey> GetAllSensorKeys() = 0;
		virtual std::optional<models::ApiKey> GetSensorKey(const std::string& id) = 0;
	};
}
