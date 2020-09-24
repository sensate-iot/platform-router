/*
 * Abstract sensor repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/services/abstractsensorrepository.h>
#include <sensateiot/services/sensorrepository.h>
#include <sensateiot/stl/smallvector.h>

#include <mongoc.h>

namespace sensateiot::services
{
	SensorRepository::SensorRepository(config::MongoDB mongodb) :
			AbstractSensorRepository(std::move(mongodb)), m_pool(util::MongoDBClientPool::GetClientPool())
	{
	}

	std::vector<models::Sensor> SensorRepository::GetAllSensors(long skip, long limit)
	{
		auto *project = BCON_NEW("$project", "{", "_id", BCON_BOOL(1), "Secret", BCON_BOOL(1), "Owner", BCON_BOOL(1),
		                         "}");
		auto *_skip = BCON_NEW("$skip", BCON_INT64(skip));
		auto *_limit = BCON_NEW("$limit", BCON_INT64(limit));
		bson_t* pipeline;

		if(limit > 0) {
			pipeline = BCON_NEW("pipeline", "[", BCON_DOCUMENT(project), BCON_DOCUMENT(_skip), BCON_DOCUMENT(_limit), "]");
		} else {
			pipeline = BCON_NEW("pipeline", "[", BCON_DOCUMENT(project), "]");
		}

		util::MongoDBClient cli(this->m_pool->Acquire());
		auto *client = cli.Get();
		auto *collection = mongoc_client_get_collection(client, this->m_mongodb.GetDatabaseName().c_str(),
		                                                Collection.data());

		auto rv = ExecuteQuery(collection, pipeline);

		mongoc_collection_destroy(collection);
		bson_destroy(_skip);
		bson_destroy(_limit);
		bson_destroy(project);
		bson_destroy(pipeline);

		return rv;
	}

	std::vector<models::Sensor> SensorRepository::GetRange(const std::vector<models::ObjectId>& ids, long skip, long limit)
	{
		std::vector<stl::SmallVector<std::uint8_t, models::ObjectId::ObjectIdSize>> byteIds;
		byteIds.reserve(ids.size());

		for(const auto& id : ids) {
			stl::SmallVector<std::uint8_t, models::ObjectId::ObjectIdSize> bytes;
			boost::multiprecision::export_bits(id.Value(), std::back_inserter(bytes), 8);
			byteIds.push_back(std::move(bytes));
		}

		bson_t *parent;
		bson_t array;
		bson_t *pipeline;

		parent = bson_new();
		bson_append_array_begin(parent, "$in", 3, &array);

		for(size_t idx = 0UL; idx < ids.size(); idx++) {
			const auto &id = byteIds.at(idx);
			bson_oid_t objid;

			bson_oid_init_from_data(&objid, id.data());
			bson_append_oid(&array, std::to_string(idx).c_str(), -1, &objid);
		}

		bson_append_array_end(parent, &array);

		auto *match = BCON_NEW("$match", "{", "_id", "{", "$in", BCON_ARRAY(&array), "}", "}");
		auto *project = BCON_NEW("$project", "{", "_id", BCON_BOOL(1), "Secret", BCON_BOOL(1), "Owner", BCON_BOOL(1),
		                         "}");
		auto *_skip = BCON_NEW("$skip", BCON_INT64(skip));
		auto *_limit = BCON_NEW("$limit", BCON_INT64(limit));

		if(limit > 0) {
			pipeline = BCON_NEW("pipeline", "[", BCON_DOCUMENT(match), BCON_DOCUMENT(project), BCON_DOCUMENT(_skip),
			                    BCON_DOCUMENT(_limit), "]");
		} else {
			pipeline = BCON_NEW("pipeline", "[", BCON_DOCUMENT(match), BCON_DOCUMENT(project), "]");
		}

		util::MongoDBClient cli(this->m_pool->Acquire());
		auto *client = cli.Get();
		auto *collection = mongoc_client_get_collection(client, this->m_mongodb.GetDatabaseName().c_str(),
		                                                Collection.data());

		auto rv = ExecuteQuery(collection, pipeline);

		mongoc_collection_destroy(collection);
		bson_destroy(parent);
		bson_destroy(match);
		bson_destroy(_skip);
		bson_destroy(_limit);
		bson_destroy(project);
		bson_destroy(pipeline);

		return rv;

	}

	std::vector<models::Sensor> SensorRepository::GetRange(const std::vector<std::string> &ids, long skip, long limit)
	{
		bson_t *parent;
		bson_t array;
		bson_t *pipeline;

		parent = bson_new();
		bson_append_array_begin(parent, "$in", 3, &array);

		for(size_t idx = 0UL; idx < ids.size(); idx++) {
			const auto &id = ids.at(idx);
			bson_oid_t objid;

			bson_oid_init_from_string(&objid, id.c_str());
			bson_append_oid(&array, std::to_string(idx).c_str(), -1, &objid);
		}

		bson_append_array_end(parent, &array);

		auto *match = BCON_NEW("$match", "{", "_id", "{", "$in", BCON_ARRAY(&array), "}", "}");
		auto *project = BCON_NEW("$project", "{", "_id", BCON_BOOL(1), "Secret", BCON_BOOL(1), "Owner", BCON_BOOL(1),
		                         "}");
		auto *_skip = BCON_NEW("$skip", BCON_INT64(skip));
		auto *_limit = BCON_NEW("$limit", BCON_INT64(limit));

		if(limit > 0) {
			pipeline = BCON_NEW("pipeline", "[", BCON_DOCUMENT(match), BCON_DOCUMENT(project), BCON_DOCUMENT(_skip),
			                    BCON_DOCUMENT(_limit), "]");
		} else {
			pipeline = BCON_NEW("pipeline", "[", BCON_DOCUMENT(match), BCON_DOCUMENT(project), "]");
		}

		util::MongoDBClient cli(this->m_pool->Acquire());
		auto *client = cli.Get();
		auto *collection = mongoc_client_get_collection(client, this->m_mongodb.GetDatabaseName().c_str(),
		                                                Collection.data());

		auto rv = ExecuteQuery(collection, pipeline);

		mongoc_collection_destroy(collection);
		bson_destroy(parent);
		bson_destroy(match);
		bson_destroy(_skip);
		bson_destroy(_limit);
		bson_destroy(project);
		bson_destroy(pipeline);

		return rv;
	}

	std::optional<models::Sensor> SensorRepository::GetSensorById(const models::ObjectId& id)
	{
		auto sensors = this->GetRange({ id }, 0, 0);

		if(sensors.empty()) {
			return {};
		}

		return std::make_optional(std::move(*sensors.begin()));
	}

	std::vector<models::Sensor>
	SensorRepository::ExecuteQuery(mongoc_collection_t *col, const bson_t *pipeline)
	{
		const bson_t *doc;

		auto *cursor = mongoc_collection_aggregate(col, MONGOC_QUERY_NONE, pipeline, nullptr, nullptr);
		std::vector<models::Sensor> sensors;

		while(mongoc_cursor_next(cursor, &doc)) {
			bson_iter_t iter;
			uint32_t length;
			const char *name;
			const char *owner;
			const bson_oid_t *sensorId;
			models::Sensor s;

			bson_iter_init(&iter, doc);
			bson_iter_next(&iter);

			do {
				std::string_view key(bson_iter_key(&iter), bson_iter_key_len(&iter));

				if(BSON_ITER_HOLDS_OID(&iter) && key == "_id") {
					sensorId = bson_iter_oid(&iter);
					models::ObjectId objId(sensorId->bytes);
					s.SetId(objId);
				}

				if(BSON_ITER_HOLDS_UTF8(&iter) && key == "Secret") {
					name = bson_iter_utf8(&iter, &length);
					s.SetSecret(name);
				}

				if(BSON_ITER_HOLDS_UTF8(&iter) && key == "Owner") {
					owner = bson_iter_utf8(&iter, &length);
					s.SetOwner(owner);
				}
			} while(bson_iter_next(&iter));


			sensors.emplace_back(std::move(s));
		}

		mongoc_cursor_destroy(cursor);


		return sensors;
	}
}
