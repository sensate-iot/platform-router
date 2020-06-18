/*
 * Abstract user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <config/database.h>
#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>

#include <boost/unordered_set.hpp>
#include <vector>
#include <string>

namespace sensateiot::services
{
	class DLL_EXPORT AbstractUserRepository {
	public:
		explicit AbstractUserRepository() = default;
		explicit AbstractUserRepository(config::PostgreSQL pgsql);
		virtual ~AbstractUserRepository() = default;

		virtual std::vector<models::User> GetAllUsers() = 0;
		virtual std::vector<models::User> GetRange(const boost::unordered_set<boost::uuids::uuid> &ids) = 0;

	protected:
		config::PostgreSQL m_pgsql;
	};
}
