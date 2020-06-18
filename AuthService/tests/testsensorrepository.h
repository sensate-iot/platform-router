/*
 * Test user repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/services/abstractsensorrepository.h>

#include <string>
#include <vector>

namespace sensateiot::test
{
	class DLL_EXPORT SensorRepository : public services::AbstractSensorRepository {
	public:
		explicit SensorRepository(config::MongoDB mongodb) {}

		std::vector<models::Sensor> GetAllSensors(long skip, long limit) override
		{
			return std::vector<models::Sensor>();
		}

		std::vector<models::Sensor> GetRange(const std::vector<std::string> &ids, long skip, long limit) override
		{
			return std::vector<models::Sensor>();
		}

		std::vector<models::Sensor> GetRange(const std::vector<models::ObjectId>& ids, long skip, long limit) override
		{
			return std::vector<models::Sensor>();
		}
	};
}
