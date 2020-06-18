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

#include <string>
#include <vector>
#include <boost/unordered_set.hpp>

namespace sensateiot::services
{
	class DLL_EXPORT AbstractApiKeyRepository {
	public:
		explicit AbstractApiKeyRepository() = default;
		explicit AbstractApiKeyRepository(config::PostgreSQL  pgsql);
		virtual ~AbstractApiKeyRepository() = default;

		virtual std::vector<std::string> GetAllSensorKeys() = 0;
		virtual std::vector<std::string> GetKeys(const std::vector<std::string>& ids) = 0;
		virtual std::vector<std::string> GetKeysByOwners(const boost::unordered_set<boost::uuids::uuid>& ids) = 0;

	protected:
		config::PostgreSQL m_pgsql;
	};
}
