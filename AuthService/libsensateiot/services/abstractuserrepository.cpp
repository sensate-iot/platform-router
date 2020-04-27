/*
 * Abstract user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/abstractuserrepository.h>

namespace sensateiot::services
{
	AbstractUserRepository::AbstractUserRepository(const config::PostgreSQL &pgsql) :
		m_pgsql(pgsql)
	{
	}
}
