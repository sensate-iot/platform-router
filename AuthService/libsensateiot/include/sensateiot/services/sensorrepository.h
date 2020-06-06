/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <config/database.h>
#include <sensateiot/util/mongodbclientpool.h>

#include <sensateiot/models/user.h>
#include <sensateiot/models/apikey.h>
#include <sensateiot/models/sensor.h>

#include <sensateiot/util/mongodbclient.h>
#include <sensateiot/stl/referencewrapper.h>
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
		using sv = std::string_view;

		constexpr static auto ObjectId = sv("_id");
		constexpr static auto Secret = sv("Secret");
		constexpr static auto Owner = sv("Owner");
		constexpr static auto Collection = sv("Sensors");

		static std::vector<models::Sensor> ExecuteQuery(mongoc_collection_t* col, const bson_t* pipeline);

		stl::ReferenceWrapper<util::MongoDBClientPool> m_pool;
	};
}
