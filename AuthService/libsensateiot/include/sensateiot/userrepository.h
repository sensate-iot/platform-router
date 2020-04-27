/*
 * User repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <pqxx/pqxx>

#include <sensateiot/abstractuserrepository.h>

namespace sensateiot::services
{
	class DLL_EXPORT UserRepository : public AbstractUserRepository {
	public:
		explicit UserRepository(const config::PostgreSQL& pgsql);
		~UserRepository() override = default;

		std::vector<models::User> GetAllUsers() override;
		std::vector<models::ApiKey> GetAllSensorKeys() override;
		std::vector<models::User> GetRange(const std::vector<std::string> &ids) override;

	private:
		pqxx::connection m_connection;
	};
}
