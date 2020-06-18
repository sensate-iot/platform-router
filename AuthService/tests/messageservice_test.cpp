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

static constexpr std::string_view json("{\"longitude\":4.774186840897145,\"latitude\":51.59384817617493,\"createdById\":\"5c7c3bbd80e8ae3154d04912\",\"createdBySecret\":\"$5e7a36d90554c9b805345533de22eafbb55b081c69fed55c8311f46b0e45527b==\",\"data\":{\"x\":{\"value\":3.7348298850142325,\"unit\":\"m/s2\"},\"y\":{\"value\":95.1696675190223,\"unit\":\"m/s2\"},\"z\":{\"value\":15.24488164994629,\"unit\":\"m/s2\"}}}");

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
	sensateiot::config::Config config;

	config.SetWorkers(3);
	config.SetInterval(1000);

	auto& mqtt = config.GetMqtt().GetPrivateBroker();
	mqtt.GetBroker().SetHostName("tcp://127.0.0.1");
	mqtt.GetBroker().SetPortNumber(1883);

	sensateiot::test::TestMqttClient client;
	client.Connect(config.GetMqtt());

	sensateiot::test::UserRepository users(config.GetDatabase().GetPostgreSQL());
	sensateiot::test::ApiKeyRepository key(config.GetDatabase().GetPostgreSQL());
	sensateiot::test::SensorRepository sensors(config.GetDatabase().GetMongoDB());
	sensateiot::mqtt::MessageService service(client, users, key, sensors, config);

	service.AddMeasurement(std::string(json));

	using namespace std::chrono_literals;
	auto t1 = std::thread([&]() {
		service.AddMeasurement("t1 Hello 1\n");
		service.AddMeasurement("t1 Hello 2\n");
		std::this_thread::sleep_for(1ms);
		service.AddMeasurement("t1 Hello 3\n");
		service.AddMeasurement("t1 Hello 4\n");
		service.Process();
	});

	auto t2 = std::thread([&]() {
		service.AddMeasurement("t2 Hello 8\n");
		service.AddMeasurement("t2 Hello\n");
		service.AddMeasurement("t2 Hello 7\n");
		service.AddMeasurement("t2 Hello 6\n");
		service.Process();
	});

	t1.join();
	t2.join();

	test_datacache();

	return -EXIT_SUCCESS;
}
