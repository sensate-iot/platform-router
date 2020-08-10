/*
 * Abstract/base SQL repository for PostgreSQL.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/services/abstractpostgresqlrepository.h>

namespace sensateiot::services
{
	AbstractPostgresqlRepository::AbstractPostgresqlRepository(config::PostgreSQL config) :
		m_pgsql(std::move(config)), m_connection(m_pgsql.GetConnectionString())
	{
	}

	void AbstractPostgresqlRepository::Reconnect()
	{
		if(this->m_connection.is_open()) {
			return;
		}

		this->m_connection.close();
		this->m_connection = pqxx::connection(this->m_pgsql.GetConnectionString());
	}
}
