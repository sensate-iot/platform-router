/*
 * Abstract user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <vector>
#include <string>

#include <sensateiot.h>

#include <config/database.h>
#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>

namespace sensateiot::services
{
	class DLL_EXPORT AbstractUserRepository {
	public:
		explicit AbstractUserRepository() = default;
		explicit AbstractUserRepository(const config::PostgreSQL& pgsql);
		virtual ~AbstractUserRepository() = default;

		virtual std::vector<models::User> GetAllUsers() = 0;
		virtual std::vector<models::User> GetRange(const std::vector<std::string>& ids) = 0;

	protected:
		config::PostgreSQL m_pgsql;
	};
}
