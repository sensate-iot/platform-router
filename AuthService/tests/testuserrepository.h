/*
 * Test user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/services/abstractuserrepository.h>

namespace sensateiot::test
{
	class DLL_EXPORT UserRepository : public services::AbstractUserRepository {
	public:
		explicit UserRepository(const config::PostgreSQL& pgsql)
		{ }

		~UserRepository() override = default;

		std::vector<models::User> GetAllUsers() override
		{
			return std::vector<models::User>();
		}

		std::vector<models::User> GetRange(const boost::unordered_set<boost::uuids::uuid> &ids) override
		{
			return std::vector<models::User>();
		}
	};
}
