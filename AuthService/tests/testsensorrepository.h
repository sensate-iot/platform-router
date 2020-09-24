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
			return this->m_sensors;
		}

		std::vector<models::Sensor> GetRange(const std::vector<std::string> &ids, long skip, long limit) override
		{
			abort();
			return std::vector<models::Sensor>();
		}

		std::vector<models::Sensor> GetRange(const std::vector<models::ObjectId>& ids, long skip, long limit) override
		{
			std::vector<models::Sensor> rv;

			for(const auto& sensor : this->m_sensors) {
				for(auto& id : ids) {
					if(sensor.GetId() != id) {
						continue;
					}

					rv.push_back(sensor);
					break;
				}
			}

			return rv;
		}

		std::optional<models::Sensor> GetSensorById(const models::ObjectId& id) override
		{
			for (auto sensor : this->m_sensors) {
				if(sensor.GetId() == id) {
					return std::make_optional(sensor);
				}
			}

			return {};
		}

		void AddSensor(const models::Sensor& sensor)
		{
			this->m_sensors.push_back(sensor);
		}

	private:
		std::vector<models::Sensor> m_sensors;
	};
}
