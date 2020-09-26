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
	class DLL_EXPORT SensorRepository final : public services::AbstractSensorRepository {
	public:
		explicit SensorRepository(config::MongoDB mongodb) {}

		std::vector<std::pair<models::ObjectId, models::Sensor>> GetAllSensors(long skip, long limit) override
		{
			return this->m_sensors;
		}

		std::vector<std::pair<models::ObjectId, models::Sensor>> GetRange(const std::vector<std::string> &ids, long skip, long limit) override
		{
			return {};
		}

		std::vector<std::pair<models::ObjectId, models::Sensor>> GetRange(const std::vector<models::ObjectId>& ids, long skip, long limit) override
		{
			std::vector<std::pair<models::ObjectId, models::Sensor>> rv;

			for(const auto& sensor : this->m_sensors) {
				for(auto& id : ids) {
					if(sensor.first != id) {
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
				if(sensor.first == id) {
					return std::make_optional(sensor.second);
				}
			}

			return {};
		}

		void AddSensor(const models::Sensor& sensor)
		{
			this->m_sensors.push_back(std::make_pair(sensor.GetId(), sensor));
		}

	private:
		std::vector<std::pair<models::ObjectId, models::Sensor>> m_sensors;
	};
}
