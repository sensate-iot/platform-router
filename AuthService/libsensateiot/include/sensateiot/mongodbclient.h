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
#include <mongocxx/client.hpp>

#include <atomic>
#include <mutex>

namespace sensateiot::util
{
	class DLL_EXPORT MongoDBClient {
	public:
		static void Init(config::MongoDB config);
		static MongoDBClient& GetClient();
		static void Destroy();

		typedef mongocxx::pool::entry PoolClient;
		typedef mongocxx::client& ClientReference;

		PoolClient acquire();

	private:
		explicit MongoDBClient(const config::MongoDB& config);

		mongocxx::instance m_instance{};
		mongocxx::uri m_uri{};
		mongocxx::pool m_pool;

		static config::MongoDB Config;
		static std::atomic<MongoDBClient*> Instance;
		static std::mutex Lock;
	};
}
