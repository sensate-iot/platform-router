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
	MongoDBClient::MongoDBClient(config::MongoDB config) :
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

	MongoDBClient &MongoDBClient::GetClient()
	{
		static MongoDBClient client(Config);
		return client;
	}

	config::MongoDB MongoDBClient::Config;
}
