/*
 * Message service integration test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>
#include <thread>
#include <chrono>
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

static void test_datacache()
{
	using namespace sensateiot;
	data::DataCache cache;
	std::vector<models::Sensor> sensors;
	boost::uuids::random_generator gen;

	for(auto idx = 0; idx < 10; idx++) {
		models::Sensor s;
		bson_oid_t oid;

		bson_oid_init(&oid, nullptr);
		sensateiot::models::ObjectId id(oid.bytes);
		s.SetId(id);
		s.SetOwner(gen());
		s.SetSecret(std::to_string(idx));
		sensors.emplace_back(std::move(s));
	}

	cache.Append(sensors);

	bson_oid_t oid;
	bson_oid_init(&oid, nullptr);
	sensateiot::models::ObjectId id(oid.bytes);

	auto sensor = cache.GetSensor(id);
	assert(!std::get<0>(sensor));
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

	using namespace std::chrono_literals;
	auto t1 = std::thread([&]() {
		service.AddMessage("t1 Hello 1\n");
		service.AddMessage("t1 Hello 2\n");
		std::this_thread::sleep_for(1ms);
		service.AddMessage("t1 Hello 3\n");
		service.AddMessage("t1 Hello 4\n");
		service.Process();
	});

	auto t2 = std::thread([&]() {
		service.AddMessage("t2 Hello 8\n");
		service.AddMessage("t2 Hello\n");
		service.AddMessage("t2 Hello 7\n");
		service.AddMessage("t2 Hello 6\n");
		service.Process();
	});

	t1.join();
	t2.join();

	test_datacache();

	return -EXIT_SUCCESS;
}
