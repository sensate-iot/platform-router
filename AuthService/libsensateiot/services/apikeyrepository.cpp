/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/database.h>

#include <sensateiot/services/apikeyrepository.h>
#include <sensateiot/util/log.h>

#include <boost/uuid/uuid.hpp>
#include <boost/uuid/uuid_io.hpp>
#include <boost/lexical_cast.hpp>

#include <string>
#include <vector>

namespace sensateiot::services
{
	ApiKeyRepository::ApiKeyRepository(const config::PostgreSQL &pgsql) : AbstractApiKeyRepository(), AbstractPostgresqlRepository(pgsql)
	{
		auto &log = util::Log::GetLog();

		if(this->m_connection.is_open()) {
			this->m_connection.prepare("get_apikey", "SELECT * FROM authorizationctx_getapikey($1)");
			log << "API keys: Connected to PostgreSQL!" << util::Log::NewLine;
		} else {
			log << "API keys: Unable to connect to PostgreSQL!" << util::Log::NewLine;
		}
	}

	std::vector<models::ApiKey> ApiKeyRepository::GetAllSensorKeys()
	{
		std::string query("SELECT * FROM authorizationctx_getapikeys()");
		this->Reconnect();

		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec(query));
		std::vector<models::ApiKey> keys;

		for(const auto &row : res) {
			models::ApiKey key;
			
			key.SetKey(row[0].as<std::string>());
			key.SetUserId(row[1].as<std::string>());
			key.SetRevoked(row[2].as<bool>());
			key.SetReadOnly(row[3].as<bool>());
			keys.emplace_back(std::move(key));
		}

		return keys;
	}

	void ApiKeyRepository::Reconnect()
	{
		if(this->m_connection.is_open()) {
			return;
		}

		AbstractPostgresqlRepository::Reconnect();
		this->m_connection.prepare("get_apikey", "SELECT * FROM authorizationctx_getapikey($1)");
	}

	std::optional<models::ApiKey> ApiKeyRepository::GetSensorKey(const std::string& id)
	{
		this->Reconnect();
		
		pqxx::nontransaction q(this->m_connection);
		pqxx::result res(q.exec_prepared("get_apikey", id));

		if(res.empty()) {
			return {};
		}

		auto row = *res.begin();
		models::ApiKey key;

		key.SetKey(row[0].as<std::string>());
		key.SetUserId(row[1].as<std::string>());
		key.SetRevoked(row[2].as<bool>());
		key.SetReadOnly(row[3].as<bool>());

		return std::make_optional(key);
	}
}
