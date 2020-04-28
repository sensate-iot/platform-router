/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/database.h>

#include <sensateiot/models/apikey.h>
#include <sensateiot/apikeyrepository.h>
#include <sensateiot/log.h>

#include <string>
#include <vector>

namespace sensateiot::services
{
	ApiKeyRepository::ApiKeyRepository(const config::PostgreSQL &pgsql)
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
		return std::vector<models::ApiKey>();
	}

	std::vector<models::ApiKey> ApiKeyRepository::GetKeys(const std::vector<std::string> &ids)
	{
		return std::vector<models::ApiKey>();
	}
}
