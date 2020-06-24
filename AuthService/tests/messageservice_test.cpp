/*
 * Message service integration test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>
#include <thread>
#include <chrono>
#include <iostream>
#include <mongoc.h>

#include <boost/uuid/uuid.hpp>
#include <boost/uuid/random_generator.hpp>

#include <sensateiot/mqtt/imqttclient.h>
#include <sensateiot/mqtt/mqttclient.h>
#include <sensateiot/data/datacache.h>

#include "testmqttclient.h"
#include "testapikeyrepository.h"
#include "testuserrepository.h"
#include "testsensorrepository.h"

static constexpr std::string_view json(R"({"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})");

static void generate_data(sensateiot::test::SensorRepository& sensors, sensateiot::test::UserRepository& users, sensateiot::test::ApiKeyRepository& keys)
{
	using namespace sensateiot;
	boost::uuids::random_generator gen;
	sensateiot::models::ObjectId testId;

	for(auto idx = 0; idx < 10; idx++) {
		models::Sensor s;
		models::User u;
		bson_oid_t oid;
		auto owner = gen();

		bson_oid_init(&oid, nullptr);
		sensateiot::models::ObjectId id(oid.bytes);
		testId = id;

		s.SetId(id);
		s.SetOwner(owner);
		s.SetSecret(std::to_string(idx));

		u.SetId(owner);
		u.SetLockout(false);
		u.SetBanned(false);

		sensors.AddSensor(s);
		users.AddUser(u);
		keys.AddKey(std::to_string(idx));
	}
}

static void test_measurement_processing()
{
	using namespace sensateiot;
	sensateiot::config::Config config;
	boost::uuids::random_generator gen;

	config.SetWorkers(3);
	config.SetInterval(1000);

	auto& mqtt = config.GetMqtt().GetPrivateBroker();
	mqtt.GetBroker().SetHostName("tcp://127.0.0.1");
	mqtt.GetBroker().SetPortNumber(1883);

	sensateiot::test::TestMqttClient client;
	client.Connect(config.GetMqtt());

	sensateiot::test::UserRepository users(config.GetDatabase().GetPostgreSQL());
	sensateiot::test::ApiKeyRepository keys(config.GetDatabase().GetPostgreSQL());
	sensateiot::test::SensorRepository sensors(config.GetDatabase().GetMongoDB());
	sensateiot::mqtt::MessageService service(client, users, keys, sensors, config);

	generate_data(sensors, users, keys);
	models::Sensor s;
	models::User u;
	std::string key = "Hello, World!";

	s.SetSecret(key);
	s.SetId("5c7c3bbd80e8ae3154d04912");
	s.SetOwner(gen());

	u.SetBanned(false);
	u.SetLockout(false);
	u.SetId(s.GetOwner());

	sensors.AddSensor(s);
	users.AddUser(u);
	keys.AddKey(key);

	service.AddMeasurement(std::string(json));
	service.AddMeasurement(std::string(json));
	service.AddMeasurement(std::string(json));
	service.AddMeasurement(std::string(json));
	service.AddMeasurement(std::string(json));

	service.Process();
}

static void test_datacache()
{
	using namespace sensateiot;
	data::DataCache cache;
	std::vector<models::Sensor> sensors;
	std::vector<models::User> users;
	std::vector<std::string> keys;
	boost::uuids::random_generator gen;
	sensateiot::models::ObjectId testId;

	for(auto idx = 0; idx < 10; idx++) {
		models::Sensor s;
		models::User u;
		bson_oid_t oid;
		auto owner = gen();

		bson_oid_init(&oid, nullptr);
		sensateiot::models::ObjectId id(oid.bytes);
		testId = id;

		s.SetId(id);
		s.SetOwner(owner);
		s.SetSecret(std::to_string(idx));

		u.SetId(owner);
		u.SetLockout(false);
		u.SetBanned(false);

		sensors.emplace_back(std::move(s));
		users.emplace_back(u);
		keys.emplace_back(std::to_string(idx));
	}

	cache.Append(sensors);
	cache.Append(keys);
	cache.Append(users);

	bson_oid_t oid;
	bson_oid_init(&oid, nullptr);
	sensateiot::models::ObjectId id(oid.bytes);

	auto sensor = cache.GetSensor(id);
	assert(!std::get<0>(sensor));
	assert(!sensor.second.has_value());

	sensor = cache.GetSensor(testId);
	assert(std::get<0>(sensor));
	assert(sensor.second.has_value());
}

int main(int argc, char** argv)
{
	mongoc_init();
	test_datacache();
	test_measurement_processing();
	mongoc_cleanup();

	return -EXIT_SUCCESS;
}
