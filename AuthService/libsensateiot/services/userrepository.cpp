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
	UserRepository::UserRepository(const config::PostgreSQL& pgsql) :
		AbstractUserRepository(pgsql), m_connection(pgsql.GetConnectionString())
	{
		auto& log = util::Log::GetLog();

		if(this->m_connection.is_open()) {
			log << "Users: Connected to PostgreSQL!" << util::Log::NewLine;
		} else {
			log << "Users: Unable to connect to PostgreSQL!" << util::Log::NewLine;
		}
	}

	std::vector<models::User> UserRepository::GetAllUsers()
	{
		std::string query("SELECT \"Users\".\"Id\",\n"
		             "       \"BillingLockout\",\n"
		             "       \"Roles\".\"NormalizedName\" = 'BANNED' as \"Banned\"\n"
		             "FROM \"Users\"\n"
		             "         INNER JOIN \"UserRoles\" ON \"Users\".\"Id\" = \"UserRoles\".\"UserId\"\n"
		             "         INNER JOIN \"Roles\" ON \"UserRoles\".\"RoleId\" = \"Roles\".\"Id\"\n"
		             "GROUP BY \"Users\".\"Id\", \"BillingLockout\", \"Roles\".\"NormalizedName\" = 'BANNED'");

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

	std::vector<models::User> UserRepository::GetRange(const boost::unordered_set<boost::uuids::uuid> &ids)
	{
		std::string rv("(");
		auto idx = 0UL;

		for(auto iter = ids.begin(); idx < ids.size(); idx++, ++iter) {
			rv += '\'' + boost::lexical_cast<std::string>(*iter) + '\'';

			if((idx + 1) != ids.size()) {
				rv += ",";
			}
		}

		rv += ")";

		std::string::size_type pos = 0u;

		std::string query("SELECT \"Users\".\"Id\",\n"
		             "       \"BillingLockout\",\n"
		             "       \"Roles\".\"NormalizedName\" = 'BANNED' as \"Banned\"\n"
		             "FROM \"Users\"\n"
		             "         INNER JOIN \"UserRoles\" ON \"Users\".\"Id\" = \"UserRoles\".\"UserId\"\n"
		             "         INNER JOIN \"Roles\" ON \"UserRoles\".\"RoleId\" = \"Roles\".\"Id\"\n"
		             "WHERE \"Users\".\"Id\" IN %\n"
		             "GROUP BY \"Users\".\"Id\", \"BillingLockout\", \"Roles\".\"NormalizedName\" = 'BANNED'");

		pos = query.find('%', pos);
		query.replace(pos, sizeof(char), rv);

		std::cout << query << std::endl;

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
}
