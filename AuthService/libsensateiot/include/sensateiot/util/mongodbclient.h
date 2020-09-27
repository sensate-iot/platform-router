/*
 * Pooled MongoDB client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <mongoc.h>

namespace sensateiot::util
{
	class MongoDBClient {
	public:
		explicit MongoDBClient(mongoc_client_pool_t* pool, mongoc_client_t* client);
		explicit MongoDBClient(std::pair<mongoc_client_pool_t*,mongoc_client_t*> client);
		~MongoDBClient();

		MongoDBClient(const MongoDBClient& other) = delete;
		MongoDBClient& operator =(const MongoDBClient& other) = delete;

		mongoc_client_t* Get();

	private:
		mongoc_client_pool_t* m_pool;
		mongoc_client_t* m_client;
	};
}
