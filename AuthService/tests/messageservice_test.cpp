/*
 * Message service integration test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>
#include <thread>
#include <chrono>

#include <sensateiot/mqtt/imqttclient.h>
#include <sensateiot/mqtt/mqttclient.h>

#include "testmqttclient.h"
#include "testapikeyrepository.h"
#include "testuserrepository.h"

int main(int argc, char** argv)
{
	sensateiot::config::Config config;

	config.SetWorkers(3);
	config.SetInterval(1000);

	auto& mqtt = config.GetMqtt().GetPrivateBroker();
	mqtt.GetBroker().SetHostName("tcp://127.0.0.1");
	mqtt.GetBroker().SetPort(1883);

	sensateiot::test::TestMqttClient client;
	client.Connect(config.GetMqtt());

	sensateiot::test::UserRepository psql(config.GetDatabase().GetPostgreSQL());
	sensateiot::test::ApiKeyRepository key(config.GetDatabase().GetPostgreSQL());
	sensateiot::mqtt::MessageService service(client, psql, key, config);

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

	return -EXIT_SUCCESS;
}
