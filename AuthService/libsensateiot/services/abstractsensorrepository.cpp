/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/services/abstractsensorrepository.h>

namespace sensateiot::services
{
	AbstractSensorRepository::AbstractSensorRepository(config::MongoDB mongodb) : m_mongodb(mongodb)
	{

	}
}
