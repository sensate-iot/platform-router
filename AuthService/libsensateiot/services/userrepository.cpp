/*
 * User repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <iostream>

#include <sensateiot/services/userrepository.h>
#include <sensateiot/util/log.h>

#include <boost/uuid/uuid.hpp>
#include <boost/uuid/uuid_io.hpp>
#include <boost/lexical_cast.hpp>

namespace sensateiot::services
{
	UserRepository::UserRepository(const config::PostgreSQL& pgsql) : AbstractUserRepository(), AbstractPostgresqlRepository(pgsql)
	{
		auto& log = util::Log::GetLog();

		if(this->m_connection.is_open()) {
			this->m_connection.prepare("get_user", "SELECT * FROM authorizationctx_getuseraccount($1)");
			log << "Users: Connected to PostgreSQL!" << util::Log::NewLine;
		} else {
			log << "Users: Unable to connect to PostgreSQL!" << util::Log::NewLine;
		}
	}

	std::vector<models::User> UserRepository::GetAllUsers()
	{
		std::string query("SELECT * FROM authorizationctx_getuseraccounts()");

		this->Reconnect();
		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::User> users;

		for(const auto& row: res) {
			models::User user;

			user.SetId(row[0].as<std::string>());
			user.SetLockout(row[1].as<bool>());
			user.SetBanned(row[2].as<bool>());

			users.emplace_back(std::move(user));
		}

		return users;
	}

	std::optional<models::User> UserRepository::GetUserById(const boost::uuids::uuid& id)
	{
		auto strId = boost::lexical_cast<std::string>(id);
		this->Reconnect();
		
		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec_prepared("get_user", strId));

		if(res.empty()) {
			return {};
		}

		auto row = *res.begin();
		models::User user;

		user.SetId(row[0].as<std::string>());
		user.SetLockout(row[1].as<bool>());
		user.SetBanned(row[2].as<bool>());

		return std::make_optional(std::move(user));
	}

	void UserRepository::Reconnect()
	{
		if(this->m_connection.is_open()) {
			return;
		}

		AbstractPostgresqlRepository::Reconnect();
		this->m_connection.prepare("get_user", "SELECT * FROM authorizationctx_getuseraccount($1)");
	}
}
