/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/database.h>

namespace sensateiot::config
{
	void PostgreSQL::SetConnectionString(const std::string& string)
	{
		this->m_connStr = string;
	}

	const std::string &PostgreSQL::GetConnectionString() const
	{
		return this->m_connStr;
	}

	const std::string &MongoDB::GetConnectionString() const
	{
		return this->m_connStr;
	}

	void MongoDB::SetConnectionString(const std::string &connStr)
	{
		this->m_connStr = connStr;
	}

	const std::string &MongoDB::GetDatabaseName() const
	{
		return this->m_dbName;
	}

	void MongoDB::SetDatabaseName(const std::string& db)
	{
		this->m_dbName = db;
	}

	PostgreSQL &Database::GetPostgreSQL()
	{
		return this->m_pg;
	}

	MongoDB &Database::GetMongoDB()
	{
		return this->m_mongo;
	}
}
