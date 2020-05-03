/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <config/database.h>
#include <sensateiot/mongodbclient.h>

#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>
#include <sensateiot/models/sensor.h>
#include <sensateiot/services/abstractsensorrepository.h>

#include <string>
#include <vector>
#include <string_view>

namespace sensateiot::services
{
	class DLL_EXPORT SensorRepository : public AbstractSensorRepository {
	public:
		explicit SensorRepository(config::MongoDB mongodb);

		std::vector<models::Sensor> GetAllSensors() override;
		std::vector<models::Sensor> GetRange(const std::vector<std::string> &ids) override;

	private:
		util::MongoDBClient::PoolClient m_client;

		using sv = std::string_view;
		constexpr static auto ObjectId = sv("_id");
		constexpr static auto Name = sv("Name");
		constexpr static auto Secret = sv("Secret");
		constexpr static auto Owner = sv("Owner");

		constexpr static auto Collection = sv("Sensors");
	};
}
