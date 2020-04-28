/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/abstractapikeyrepository.h>

namespace sensateiot::services
{
	AbstractApiKeyRepository::AbstractApiKeyRepository(const config::PostgreSQL &pgsql) :
		m_pgsql(pgsql)
	{
	}
}
