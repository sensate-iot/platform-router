/*
 * Abstract/base SQL repository for PostgreSQL.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <pqxx/pqxx>
#include <config/database.h>

namespace sensateiot::services
{
	class AbstractPostgresqlRepository {
	public:
		explicit AbstractPostgresqlRepository() = default;
		explicit AbstractPostgresqlRepository(config::PostgreSQL config);
		virtual ~AbstractPostgresqlRepository() = default;
		
	protected:
		config::PostgreSQL m_pgsql;
		pqxx::connection m_connection;

		/* Methods */
		virtual void Reconnect();
	};
}
