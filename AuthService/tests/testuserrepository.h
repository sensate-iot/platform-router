/*
 * Test user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/abstractuserrepository.h>

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

		std::vector<models::ApiKey> GetAllSensorKeys() override
		{
			return std::vector<models::ApiKey>();
		}

		std::vector<models::User> GetRange(const std::vector<std::string> &ids) override
		{
			return std::vector<models::User>();
		}
	};
}
