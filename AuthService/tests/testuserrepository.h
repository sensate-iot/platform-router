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
			std::vector<models::User> rv;

			for(const auto& user : this->m_users) {
				for(auto& id : ids) {
					if(user.GetId() != id) {
						continue;
					}

					rv.push_back(user);
					break;
				}
			}

			return rv;
		}

		void AddUser(const models::User& user)
		{
			this->m_users.push_back(user);
		}

	private:
		std::vector<models::User> m_users;
	};
}
