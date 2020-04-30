/*
 * Pooled MongoDB client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/mongodbclient.h>
#include <sensateiot/log.h>

#include <iostream>

namespace sensateiot::util
{
	MongoDBClient::MongoDBClient(const config::MongoDB& config) :
		m_uri(config.GetConnectionString()), m_pool(m_uri)
	{
		auto testClient = this->m_pool.acquire();

		auto& lg = util::Log::GetLog();

		if(testClient) {
			lg << "MongoDB connected!" << Log::NewLine;
		}
	}

	void MongoDBClient::Init(config::MongoDB config)
	{
		Config = std::move(config);
	}

	MongoDBClient& MongoDBClient::GetClient()
	{
		if(Instance == nullptr) {
			std::lock_guard<std::mutex> l(Lock);

			if(Instance == nullptr) {
				Instance = new MongoDBClient(Config);
			}
		}

		return *Instance;
	}

	MongoDBClient::PoolClient MongoDBClient::acquire()
	{
		return this->m_pool.acquire();
	}

	void MongoDBClient::Destroy()
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

	std::atomic<MongoDBClient*> MongoDBClient::Instance { nullptr };
	std::mutex MongoDBClient::Lock;
	config::MongoDB MongoDBClient::Config;
}
