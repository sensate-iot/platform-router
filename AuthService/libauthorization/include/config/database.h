/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <string>

namespace sensateiot::config
{
	class DLL_EXPORT MongoDB {
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

	class DLL_EXPORT PostgreSQL {
	public:
		[[nodiscard]]
		const std::string &GetConnectionString() const;
		void SetConnectionString(const std::string &mConnStr);

	private:
		std::string m_connStr;
	};

	class DLL_EXPORT Database {
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
