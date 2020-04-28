/*
 * Pooled MongoDB client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/database.h>

#include <mongocxx/instance.hpp>
#include <mongocxx/pool.hpp>

namespace sensateiot::util
{
	class DLL_EXPORT MongoDBClient {
	public:
		static void Init(config::MongoDB config);
		static MongoDBClient& GetClient();

	private:
		explicit MongoDBClient(config::MongoDB config);

		mongocxx::instance m_instance{};
		mongocxx::uri m_uri{};
		mongocxx::pool m_pool;

		static config::MongoDB Config;
	};
}
