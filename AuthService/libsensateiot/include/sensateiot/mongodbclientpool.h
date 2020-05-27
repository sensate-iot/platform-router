/*
 * Pooled MongoDB client pool.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <mongoc.h>

#include <sensateiot/util/mongodbclient.h>
#include <config/database.h>

#include <atomic>
#include <mutex>
#include <utility>

namespace sensateiot::util
{
	class DLL_EXPORT MongoDBClientPool {
	public:
		~MongoDBClientPool();

		static void Init(config::MongoDB config);
		static MongoDBClientPool& GetClientPool();
		static void Destroy();

		std::pair<mongoc_client_pool_t*, mongoc_client_t*> Acquire();

	private:
		explicit MongoDBClientPool(const config::MongoDB& config);

		mongoc_uri_t* m_uri;
		mongoc_client_pool_t* m_pool;

		static config::MongoDB Config;
		static std::atomic<MongoDBClientPool*> Instance;
		static std::mutex Lock;
	};
}
