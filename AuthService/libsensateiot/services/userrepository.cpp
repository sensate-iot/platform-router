/*
 * User repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <iostream>

#include <sensateiot/userrepository.h>
#include <sensateiot/log.h>

namespace sensateiot::services
{
	UserRepository::UserRepository(const config::PostgreSQL &pgsql) :
		AbstractUserRepository(pgsql), m_connection(pgsql.GetConnectionString())
	{
		auto& log = util::Log::GetLog();

		if(this->m_connection.is_open()) {
			log << "Connected to PostgreSQL!" << util::Log::NewLine;
		} else {
			log << "Unable to connect to PostgreSQL!" << util::Log::NewLine;
		}
	}

	std::vector<models::User> UserRepository::GetAllUsers()
	{
		auto query = "SELECT \"Users\".\"Id\",\n"
		             "       \"BillingLockout\",\n"
		             "       \"Roles\".\"NormalizedName\" = 'BANNED' as \"Banned\"\n"
		             "FROM \"Users\"\n"
		             "         INNER JOIN \"UserRoles\" ON \"Users\".\"Id\" = \"UserRoles\".\"UserId\"\n"
		             "         INNER JOIN \"Roles\" ON \"UserRoles\".\"RoleId\" = \"Roles\".\"Id\"\n"
		             "GROUP BY \"Users\".\"Id\", \"BillingLockout\", \"Roles\".\"NormalizedName\" = 'BANNED'";

		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::User> users;

		for(auto row: res) {
			models::User user;

			user.SetId(row[0].as<std::string>());
			user.SetLockout(row[1].as<bool>());
			user.SetBanned(row[2].as<bool>());

			users.emplace_back(std::move(user));
		}

		return users;
	}

	std::vector<models::ApiKey> UserRepository::GetAllSensorKeys()
	{
		auto query = "SELECT \"UserId\", \"ApiKey\", \"Revoked\"\n"
		             "FROM \"ApiKeys\"\n"
		             "WHERE \"Type\" = 0";

		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::ApiKey> keys;

		for(auto row : res) {
			models::ApiKey key;

			key.SetUserId(row[0].as<std::string>());
			key.SetKey(row[1].as<std::string>());
			key.SetRevoked(row[2].as<bool>());

			keys.emplace_back(std::move(key));
		}

		return keys;
	}

	std::vector<models::User> UserRepository::GetRange(const std::vector<std::string> &ids)
	{
		std::string rv("(");

		for(auto idx = 0U; idx < ids.size(); idx++) {
			rv += '\'' + ids[idx] + '\'';

			if((idx + 1) != ids.size()) {
				rv += ",";
			}
		}

		rv += ")";

		std::string::size_type pos = 0u;

		auto query = std::string("SELECT \"Users\".\"Id\",\n"
		             "       \"BillingLockout\",\n"
		             "       \"Roles\".\"NormalizedName\" = 'BANNED' as \"Banned\"\n"
		             "FROM \"Users\"\n"
		             "         INNER JOIN \"UserRoles\" ON \"Users\".\"Id\" = \"UserRoles\".\"UserId\"\n"
		             "         INNER JOIN \"Roles\" ON \"UserRoles\".\"RoleId\" = \"Roles\".\"Id\"\n"
		             "WHERE \"Users\".\"Id\" IN %\n"
		             "GROUP BY \"Users\".\"Id\", \"BillingLockout\", \"Roles\".\"NormalizedName\" = 'BANNED'");

		pos = query.find('%', pos);
		query.replace(pos, sizeof(char), rv);


		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::User> users;

		for(auto row: res) {
			models::User user;

			user.SetId(row[0].as<std::string>());
			user.SetLockout(row[1].as<bool>());
			user.SetBanned(row[2].as<bool>());

			users.emplace_back(std::move(user));
		}

		return users;
	}
}
