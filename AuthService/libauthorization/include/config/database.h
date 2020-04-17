/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>

namespace sensateiot::auth::config
{
	class MongoDB {
	public:
		[[nodiscard]]
		const std::string& GetConnectionString() const;
		void SetConnectionString(const std::string &connStr);

		[[nodiscard]]
		const std::string& GetDatabaseName() const;
		void SetDatabaseName(const std::string &db);

	private:
		std::string m_dbName;
		std::string m_connStr;
	};

	class PostgreSQL {
	public:
		[[nodiscard]]
		const std::string &GetConnectionString() const;
		void SetConnectionString(const std::string &mConnStr);

	private:
		std::string m_connStr;
	};

	class Database {
	public:
		[[nodiscard]]
		PostgreSQL& GetPostgreSQL();

		[[nodiscard]]
		MongoDB& GetMongoDB();

	private:
		PostgreSQL m_pg;
		MongoDB m_mongo;
	};
}
