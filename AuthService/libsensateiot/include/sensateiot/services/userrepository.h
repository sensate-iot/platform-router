/*
 * User repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <pqxx/pqxx>

#include <sensateiot/services/abstractuserrepository.h>
#include <sensateiot/services/abstractpostgresqlrepository.h>

namespace sensateiot::services
{
	class DLL_EXPORT UserRepository : public AbstractUserRepository, public AbstractPostgresqlRepository {
	public:
		explicit UserRepository(const config::PostgreSQL& pgsql);
		~UserRepository() override = default;

		std::vector<std::pair<models::User::IdType, models::User>> GetAllUsers() override;
		std::optional<models::User> GetUserById(const boost::uuids::uuid& id) override;
		
	protected:
		void Reconnect() override;
	};
}
