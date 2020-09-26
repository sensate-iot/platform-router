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
#include <optional>

namespace sensateiot::services
{
	class DLL_EXPORT AbstractSensorRepository {
	public:
		typedef std::pair<models::ObjectId, models::Sensor> ValueType;

		explicit AbstractSensorRepository() = default;
		explicit AbstractSensorRepository(config::MongoDB mongodb);
		virtual ~AbstractSensorRepository() = default;

		virtual std::vector<ValueType> GetAllSensors(long skip, long limit) = 0;
		virtual std::optional<models::Sensor> GetSensorById(const models::ObjectId& id) = 0;
		virtual std::vector<ValueType> GetRange(const std::vector<models::ObjectId>& ids, long skip, long limit) = 0;
		virtual std::vector<ValueType> GetRange(const std::vector<std::string>& ids, long skip, long limit) = 0;

	protected:
		config::MongoDB m_mongodb;
	};
}
