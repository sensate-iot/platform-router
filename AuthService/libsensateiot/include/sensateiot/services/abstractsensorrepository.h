/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/database.h>

#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>
#include <sensateiot/models/sensor.h>

#include <string>
#include <vector>

namespace sensateiot::services
{
	class DLL_EXPORT AbstractSensorRepository {
	public:
		explicit AbstractSensorRepository() = default;
		explicit AbstractSensorRepository(config::MongoDB mongodb);
		virtual ~AbstractSensorRepository() = default;

		virtual std::vector<models::ApiKey> GetAllSensors() = 0;
		virtual std::vector<models::ApiKey> GetRange(const std::vector<std::string>& ids) = 0;

	protected:
		config::MongoDB m_mongodb;
	};
}
