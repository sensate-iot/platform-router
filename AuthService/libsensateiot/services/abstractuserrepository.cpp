/*
 * Abstract user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/abstractuserrepository.h>

namespace sensateiot::services
{
	AbstractUserRepository::AbstractUserRepository(config::PostgreSQL pgsql) :
		m_pgsql(std::move(pgsql))
	{
	}
}
