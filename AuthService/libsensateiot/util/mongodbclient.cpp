/*
 * Pooled MongoDB client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/mongodbclient.h>

namespace sensateiot::util
{
	MongoDBClient::MongoDBClient(mongoc_client_pool_t* pool, mongoc_client_t *client) : m_pool(pool), m_client(client)
	{
	}

	MongoDBClient::MongoDBClient(std::pair<mongoc_client_pool_t *, mongoc_client_t *> client) :
		m_pool(client.first), m_client(client.second)
	{

	}

	MongoDBClient::~MongoDBClient()
	{
		mongoc_client_pool_push(this->m_pool, this->m_client);
	}

	mongoc_client_t *MongoDBClient::Get()
	{
		return this->m_client;
	}

}
