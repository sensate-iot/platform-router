/*
 * API key repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/services/abstractapikeyrepository.h>

#include <utility>

namespace sensateiot::services
{
	AbstractApiKeyRepository::AbstractApiKeyRepository(config::PostgreSQL pgsql) :
		AbstractPostgresqlRepository(std::move(pgsql))
	{
	}
}
