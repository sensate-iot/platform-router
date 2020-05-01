/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/services/abstractsensorrepository.h>
#include <sensateiot/services/sensorrepository.h>

namespace sensateiot::services
{
	SensorRepository::SensorRepository(config::MongoDB mongodb) :
		AbstractSensorRepository(mongodb), m_client(util::MongoDBClient::GetClient().acquire())
	{
	}

	std::vector<models::Sensor> SensorRepository::GetAllSensors()
	{
		return std::vector<models::Sensor>();
	}

	std::vector<models::Sensor> SensorRepository::GetRange(const std::vector<std::string> &ids)
	{
		return std::vector<models::Sensor>();
	}
}
