/*
 * Message service integration test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>
#include <thread>
#include <chrono>

#include <config/config.h>
#include <config/database.h>
#include <sensateiot/mongodbclient.h>

int main(int argc, char** argv)
{
	sensateiot::config::Config config;

	config.SetWorkers(3);
	config.SetInterval(1000);
	config.GetDatabase().GetMongoDB().SetConnectionString("mongodb://root:root@127.0.0.1:27017/admin");
	config.GetDatabase().GetMongoDB().SetDatabaseName("Sensate");

	sensateiot::util::MongoDBClient::Init(config.GetDatabase().GetMongoDB());

	using namespace std::chrono_literals;

	auto t1 = std::thread([&]() {
		sensateiot::util::MongoDBClient::GetClient();
		std::this_thread::sleep_for(1ms);
	});

	auto t2 = std::thread([&]() {
		sensateiot::util::MongoDBClient::GetClient();
		std::this_thread::sleep_for(1ms);
	});

	auto t3 = std::thread([&]() {
		sensateiot::util::MongoDBClient::GetClient();
		std::this_thread::sleep_for(1ms);
	});

	auto t4 = std::thread([&]() {
		sensateiot::util::MongoDBClient::GetClient();
		std::this_thread::sleep_for(1ms);
	});

	t1.join();
	t2.join();
	t3.join();
	t4.join();

	sensateiot::util::MongoDBClient::Destroy();

	return -EXIT_SUCCESS;
}
