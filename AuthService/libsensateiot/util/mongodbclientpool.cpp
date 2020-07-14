/*
 * Pooled MongoDB client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/mongodbclientpool.h>
#include <sensateiot/util/log.h>

namespace sensateiot::util
{
	MongoDBClientPool::MongoDBClientPool(const config::MongoDB& config) :
		m_uri(nullptr), m_pool(nullptr)
	{
		mongoc_init();
		this->m_uri = mongoc_uri_new(config.GetConnectionString().c_str());
		this->m_pool = mongoc_client_pool_new(this->m_uri);

//		auto* client = mongoc_client_new_from_uri(this->m_uri);
//		mongoc_client_destroy(client);

//		auto testClient = this->m_pool.acquire();
//
//		auto& lg = util::Log::GetLog();
//
//		if(testClient) {
//			lg << "MongoDB connected!" << Log::NewLine;
//		}
	}

	void MongoDBClientPool::Init(config::MongoDB config)
	{
		Config = std::move(config);
	}

	MongoDBClientPool& MongoDBClientPool::GetClientPool()
	{
		if(Instance == nullptr) {
			std::lock_guard<std::mutex> l(Lock);

			if(Instance == nullptr) {
				Instance = new MongoDBClientPool(Config);
			}
		}

		return *Instance;
	}

	void MongoDBClientPool::Destroy()
	{
		if(Instance != nullptr) {
			std::lock_guard<std::mutex> l(Lock);

			if(Instance != nullptr) {
				auto *ptr = Instance.load();
				delete ptr;
				Instance = nullptr;
			}
		}
	}

	MongoDBClientPool::~MongoDBClientPool()
	{
		mongoc_uri_destroy(this->m_uri);
		mongoc_client_pool_destroy(this->m_pool);
		mongoc_cleanup();
	}

	std::atomic<MongoDBClientPool*> MongoDBClientPool::Instance {nullptr };

	std::pair<mongoc_client_pool_t *, mongoc_client_t *> MongoDBClientPool::Acquire()
	{
		auto* client = mongoc_client_pool_pop(this->m_pool);
		return std::pair<mongoc_client_pool_t *, mongoc_client_t *>(this->m_pool, client);
	}

	std::mutex MongoDBClientPool::Lock;
	config::MongoDB MongoDBClientPool::Config{};
}
