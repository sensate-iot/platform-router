/*
 * 
 */

#include <cstdlib>
#include <thread>
#include <chrono>
#include <vector>
#include <fstream>
#include <iostream>
#include <mongoc.h>

#include <rapidjson/rapidjson.h>
#include <rapidjson/document.h>

#include <google/protobuf/any.h>

#include <boost/uuid/uuid.hpp>
#include <boost/uuid/random_generator.hpp>

#include <sensateiot/mqtt/mqttclient.h>
#include <sensateiot/mqtt/imqttclient.h>
#include <sensateiot/mqtt/mqttclient.h>
#include <sensateiot/mqtt/mqttinternalcallback.h>
#include <sensateiot/data/datacache.h>

#include <sensateiot/services/messageservice.h>
#include <sensateiot/util/mongodbclientpool.h>
#include <sensateiot/services/userrepository.h>
#include <sensateiot/services/apikeyrepository.h>
#include <sensateiot/services/sensorrepository.h>

#include "testmqttclient.h"

static constexpr std::string_view format(R"({"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"%ID%","createdBySecret":"%S%","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})");

static std::vector<std::string> measurements;

namespace sensateiot::test
{
	bool replace(std::string& str, const std::string& from, const std::string& to)
	{
		size_t start_pos = str.find(from);

		if(start_pos == std::string::npos) {
			return false;
		}

		str.replace(start_pos, from.length(), to);
		return true;
	}
}

static void ParseMqtt(sensateiot::config::Config& config, nlohmann::json &j)
{
	config.SetInterval(j["Interval"]);
	config.SetWorkers(j["Workers"]);
	config.SetInternalBatchSize(j["InternalBatchSize"]);

	config.GetMqtt().GetPrivateBroker()
			.GetBroker().SetHostName(j["Mqtt"]["InternalBroker"]["Host"]);
	config.GetMqtt().GetPrivateBroker()
			.GetBroker().SetPortNumber(j["Mqtt"]["InternalBroker"]["Port"]);
	config.GetMqtt().GetPrivateBroker()
			.GetBroker().SetUsername(j["Mqtt"]["InternalBroker"]["Username"]);
	config.GetMqtt().GetPrivateBroker()
			.GetBroker().SetPassword(j["Mqtt"]["InternalBroker"]["Password"]);
	config.GetMqtt().GetPrivateBroker()
			.GetBroker().SetSsl(j["Mqtt"]["InternalBroker"]["Ssl"] == "true");
	config.GetMqtt().GetPrivateBroker()
			.SetBulkMeasurementTopic(j["Mqtt"]["InternalBroker"]["InternalBulkMeasurementTopic"]);
	config.GetMqtt().GetPrivateBroker()
			.SetMeasurementTopic(j["Mqtt"]["InternalBroker"]["InternalMeasurementTopic"]);
	config.GetMqtt().GetPrivateBroker()
			.SetMessageTopic(j["Mqtt"]["InternalBroker"]["InternalMessageTopic"]);

	config.GetMqtt().GetPublicBroker()
			.GetBroker().SetHostName(j["Mqtt"]["PublicBroker"]["Host"]);
	config.GetMqtt().GetPublicBroker()
			.GetBroker().SetPortNumber(j["Mqtt"]["PublicBroker"]["Port"]);
	config.GetMqtt().GetPublicBroker()
			.GetBroker().SetUsername(j["Mqtt"]["PublicBroker"]["Username"]);
	config.GetMqtt().GetPublicBroker()
			.GetBroker().SetPassword(j["Mqtt"]["PublicBroker"]["Password"]);
	config.GetMqtt().GetPublicBroker()
			.GetBroker().SetSsl(j["Mqtt"]["PublicBroker"]["Ssl"] == "true");
	config.GetMqtt().GetPublicBroker()
			.SetBulkMeasurementTopic(j["Mqtt"]["PublicBroker"]["BulkMeasurementTopic"]);
	config.GetMqtt().GetPublicBroker()
			.SetMeasurementTopic(j["Mqtt"]["PublicBroker"]["MeasurementTopic"]);
	config.GetMqtt().GetPublicBroker()
			.SetMessageTopic(j["Mqtt"]["PublicBroker"]["MessageTopic"]);
}

static void ParseDatabase(sensateiot::config::Config& config, nlohmann::json &json)
{
	config.GetDatabase().GetPostgreSQL()
			.SetConnectionString(json["Database"]["PgSQL"]["ConnectionString"]);
	config.GetDatabase().GetMongoDB()
			.SetDatabaseName(json["Database"]["MongoDB"]["DatabaseName"]);
	config.GetDatabase().GetMongoDB()
			.SetConnectionString(json["Database"]["MongoDB"]["ConnectionString"]);
}

static void ParseLogging(sensateiot::config::Config& config, nlohmann::json &json)
{
	config.GetLogging().SetLevel(json["Logging"]["Level"]);
	config.GetLogging().SetPath(json["Logging"]["File"]);
}

static void generate_measurements(std::string path, int max)
{
	std::ifstream file(path);
	std::stringstream sstream;

	sstream << file.rdbuf() << std::endl;
	std::string json(sstream.str());

	rapidjson::Document doc;
	doc.Parse(json.c_str());

	auto idx = 0;
	for(auto* it = doc.Begin(); idx < max && it != doc.End(); ++it, ++idx) {
		auto& value = *it;
		std::string id = value["sensor"].GetString();
		std::string secret = value["secret"].GetString();
		std::string measurement(format.begin(), format.end());

		sensateiot::test::replace(measurement, "%ID%", id);
		sensateiot::test::replace(measurement, "%S%", secret);

		measurements.emplace_back(std::move(measurement));
	}
}

static void ParseConfig(sensateiot::config::Config& config, const std::string& path)
{
	using namespace nlohmann;
	std::ifstream file(path);

	if(!file.good()) {
		throw std::runtime_error("Config file not found!");
	}

	std::string content(
			(std::istreambuf_iterator<char>(file)),
			std::istreambuf_iterator<char>());

	try {
		auto j = json::parse(content);

		ParseMqtt(config, j);
		ParseDatabase(config, j);
		ParseLogging(config, j);
	} catch(json::exception &ex) {
		std::cerr << "Unable to parse configuration file: " <<
				  ex.what() << std::endl;
	}
}

static void test_messageservice(std::string path)
{
	using namespace sensateiot;
	config::Config config;

	ParseConfig(config, path);
	util::Log::StartLogging(config.GetLogging());
	util::MongoDBClientPool::Init(config.GetDatabase().GetMongoDB());
	
	sensateiot::mqtt::MqttInternalCallback icb;
	auto ihost = config.GetMqtt().GetPublicBroker().GetBroker().GetUri();
	sensateiot::mqtt::InternalMqttClient iclient(ihost, "3lasdfjlas", std::move(icb));
	iclient.Connect(config.GetMqtt());
	
	services::UserRepository users(config.GetDatabase().GetPostgreSQL());
	services::ApiKeyRepository keys(config.GetDatabase().GetPostgreSQL());
	services::SensorRepository sensors(config.GetDatabase().GetMongoDB());

	sensateiot::mqtt::MessageService service(iclient, users, keys, sensors, config);

	auto& log = util::Log::GetLog();
	auto start = boost::chrono::system_clock::now();

	for(auto& m : measurements) {
		service.AddMeasurement(m);
	}

	auto diff = boost::chrono::system_clock::now() - start;
	auto duration = boost::chrono::duration_cast<boost::chrono::milliseconds>(diff);

	log << "Adding measurements took: " << std::to_string(duration.count()) << "ms." << util::Log::NewLine;

	service.Process();
	service.Process();
	
	for(auto& m : measurements) {
		service.AddMeasurement(m);
	}
	
	service.Process();
}

int main(int argc, char** argv)
{
	mongoc_init();
	mongoc_cleanup();

	auto max = atoi(argv[3]);

	generate_measurements(argv[1], max);
	test_messageservice(argv[2]);
	google::protobuf::ShutdownProtobufLibrary();

	return -EXIT_SUCCESS;
}
