/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/services/abstractsensorrepository.h>
#include <sensateiot/services/sensorrepository.h>

#include <mongocxx/pipeline.hpp>
#include <bsoncxx/json.hpp>

#include <bsoncxx/builder/basic/document.hpp>
#include <bsoncxx/builder/basic/array.hpp>
#include <bsoncxx/builder/basic/kvp.hpp>
#include <iostream>

using bsoncxx::builder::basic::kvp;
using bsoncxx::builder::basic::make_document;
using bsoncxx::builder::basic::make_array;

namespace sensateiot::services
{
	SensorRepository::SensorRepository(config::MongoDB mongodb) :
		AbstractSensorRepository(mongodb), m_client(util::MongoDBClient::GetClient().acquire())
	{
	}

	std::vector<models::Sensor> SensorRepository::GetAllSensors()
	{
		mongocxx::pipeline stages;
		auto db = this->m_client->database(this->m_mongodb.GetDatabaseName());

		stages.project(
			make_document(
				kvp(std::string(ObjectId), 1),
				kvp(std::string(Name), 1),
				kvp(std::string(Owner), 1),
				kvp(std::string(Secret), 1)
			)
		);

		auto cursor = db[Collection.data()].aggregate(stages);
		std::vector<models::Sensor> sensors;

		for(auto&& doc: cursor) {
			models::Sensor sensor;

			sensor.SetId(doc[ObjectId.data()].get_oid().value.to_string());
			sensor.SetOwner(doc[Owner.data()].get_utf8().value.to_string());
			sensor.SetSecret(doc[Secret.data()].get_utf8().value.to_string());
			sensor.SetName(doc[Name.data()].get_utf8().value.to_string());

			sensors.emplace_back(sensor);
		}

		return sensors;
	}

	std::vector<models::Sensor> SensorRepository::GetRange(const std::vector<std::string> &ids)
	{
		bsoncxx::builder::basic::array ary{};
		mongocxx::pipeline stages;
		auto db = this->m_client->database(this->m_mongodb.GetDatabaseName());

		for(auto&& id : ids) {
			bsoncxx::oid oid(id);
			ary.append(oid);
		}

		stages.match(
			make_document(
				kvp(std::string(ObjectId),make_document(
					kvp("$in", ary)
					)
				)
		));

		stages.project(
				make_document(
						kvp(std::string(ObjectId), 1),
						kvp(std::string(Name), 1),
						kvp(std::string(Owner), 1),
						kvp(std::string(Secret), 1)
				)
		);

		auto cursor = db[Collection.data()].aggregate(stages);
		std::vector<models::Sensor> sensors;

		for(auto&& doc: cursor) {
			models::Sensor sensor;

			sensor.SetId(doc[ObjectId.data()].get_oid().value.to_string());
			sensor.SetOwner(doc[Owner.data()].get_utf8().value.to_string());
			sensor.SetSecret(doc[Secret.data()].get_utf8().value.to_string());
			sensor.SetName(doc[Name.data()].get_utf8().value.to_string());

			sensors.emplace_back(sensor);
		}

		return sensors;
	}
}
