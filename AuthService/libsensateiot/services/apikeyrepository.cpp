/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/database.h>

#include <sensateiot/models/apikey.h>
#include <sensateiot/services/apikeyrepository.h>
#include <sensateiot/log.h>

#include <string>
#include <vector>
#include <iostream>

namespace sensateiot::services
{
	ApiKeyRepository::ApiKeyRepository(config::PostgreSQL pgsql) :
		AbstractApiKeyRepository(std::move(pgsql)), m_connection(pgsql.GetConnectionString())
	{
		auto& log = util::Log::GetLog();

		if(this->m_connection.is_open()) {
			log << "Connected to PostgreSQL!" << util::Log::NewLine;
		} else {
			log << "Unable to connect to PostgreSQL!" << util::Log::NewLine;
		}
	}

	std::vector<models::ApiKey> ApiKeyRepository::GetAllSensorKeys()
	{
		std::string query("SELECT \"UserId\", \"ApiKey\", \"Revoked\"\n"
		             "FROM \"ApiKeys\"\n"
		             "WHERE \"Type\" = 0");

		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::ApiKey> keys;

		for(const auto& row : res) {
			models::ApiKey key;

			key.SetUserId(row[0].as<std::string>());
			key.SetKey(row[1].as<std::string>());
			key.SetRevoked(row[2].as<bool>());

			keys.emplace_back(std::move(key));
		}

		return keys;
	}

	std::vector<models::ApiKey> ApiKeyRepository::GetKeys(const std::vector<std::string> &ids)
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
		std::string query(
				"SELECT \"ApiKeys\".\"Id\", \"ApiKeys\".\"ApiKey\", \"ApiKeys\".\"Type\", \"ApiKeys\".\"Revoked\"\n"
					  "FROM \"ApiKeys\"\n"
					  "WHERE \"ApiKeys\".\"ApiKey\" IN ?");

		pos = query.find('?', pos);
		query.replace(pos, sizeof(char), rv);


		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::ApiKey> keys;

		for(const auto& row: res) {
			models::ApiKey key;

			key.SetUserId(row[0].as<std::string>());
			key.SetKey(row[1].as<std::string>());
			key.SetType(static_cast<models::ApiKey::Type>(row[2].as<std::uint16_t>()));
			key.SetRevoked(row[3].as<bool>());

			std::cout << key.GetKey() << std::endl;

			keys.emplace_back(std::move(key));
		}

		return keys;
	}
}
