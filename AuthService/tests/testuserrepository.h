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

		std::vector<std::pair<models::User::IdType, models::User>> GetAllUsers() override
		{
			std::vector<std::pair<models::User::IdType, models::User>> users;
			
			for (auto user : this->m_users) {
				users.push_back(std::make_pair(user.GetId(), user));
			}

			return users;
		}
		
		std::optional<models::User> GetUserById(const boost::uuids::uuid& id) override
		{
			for (auto user : this->m_users) {
				if(user.GetId() == id) {
					return std::make_optional(user);
				}
			}

			return {};
		}

		void AddUser(const models::User& user)
		{
			this->m_users.push_back(user);
		}

	private:
		std::vector<models::User> m_users;
	};
}
