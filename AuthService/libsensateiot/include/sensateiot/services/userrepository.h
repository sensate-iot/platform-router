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

		std::vector<models::User> GetAllUsers() override;
		std::vector<models::User> GetRange(const boost::unordered_set<boost::uuids::uuid> &ids) override;
	};
}
