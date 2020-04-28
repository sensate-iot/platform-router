/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/abstractapikeyrepository.h>

#include <utility>

namespace sensateiot::services
{
	AbstractApiKeyRepository::AbstractApiKeyRepository(config::PostgreSQL pgsql) :
		m_pgsql(std::move(pgsql))
	{
	}
}
