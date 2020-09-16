/*
 * Config parser implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/configparser.h>

#include <json.hpp>

#include <string>
#include <fstream>
#include <iostream>

namespace sensateiot::parser
{
	static void ParseMqtt(nlohmann::json &j, config::Config& cfg)
	{
		cfg.SetInterval(j["Interval"]);
		cfg.SetWorkers(j["Workers"]);

		cfg.GetMqtt().GetPrivateBroker()
				.GetBroker().SetHostName(j["Mqtt"]["InternalBroker"]["Host"]);
		cfg.GetMqtt().GetPrivateBroker()
				.GetBroker().SetPortNumber(j["Mqtt"]["InternalBroker"]["Port"]);
		cfg.GetMqtt().GetPrivateBroker()
				.GetBroker().SetUsername(j["Mqtt"]["InternalBroker"]["Username"]);
		cfg.GetMqtt().GetPrivateBroker()
				.GetBroker().SetPassword(j["Mqtt"]["InternalBroker"]["Password"]);
		cfg.GetMqtt().GetPrivateBroker()
				.GetBroker().SetSsl(j["Mqtt"]["InternalBroker"]["Ssl"] == "true");
		cfg.GetMqtt().GetPrivateBroker()
				.SetBulkMeasurementTopic(j["Mqtt"]["InternalBroker"]["InternalBulkMeasurementTopic"]);
		cfg.GetMqtt().GetPrivateBroker()
				.SetBulkMessageTopic(j["Mqtt"]["InternalBroker"]["InternalBulkMessageTopic"]);
		cfg.GetMqtt().GetPrivateBroker()
				.SetClientId(j["Mqtt"]["InternalBroker"]["ClientId"]);
		cfg.GetMqtt().GetPrivateBroker()
				.SetCommandTopic(j["Mqtt"]["InternalBroker"]["InternalCommandTopic"]);
	}

	static void ParseDatabase(nlohmann::json &json, config::Config& cfg)
	{
		cfg.GetDatabase().GetPostgreSQL()
				.SetConnectionString(json["Database"]["PgSQL"]["ConnectionString"]);
		cfg.GetDatabase().GetMongoDB()
				.SetDatabaseName(json["Database"]["MongoDB"]["DatabaseName"]);
		cfg.GetDatabase().GetMongoDB()
				.SetConnectionString(json["Database"]["MongoDB"]["ConnectionString"]);
	}

	static void ParseLogging(nlohmann::json &json, config::Config& cfg)
	{
		cfg.GetLogging().SetLevel(json["Logging"]["Level"]);
		cfg.GetLogging().SetPath(json["Logging"]["File"]);
	}

	config::Config Parse(const std::string& path)
	{
		using namespace nlohmann;
		std::ifstream file(path);
		config::Config cfg;

		if(!file.good()) {
			throw std::runtime_error("Config file not found!");
		}

		std::string content(
				(std::istreambuf_iterator<char>(file)),
				std::istreambuf_iterator<char>());

		try {
			auto j = json::parse(content);

			cfg.SetInternalBatchSize(j["InternalBatchSize"]);
			cfg.SetBindAddress(j["BindAddress"]);
			cfg.SetHttpPort(j["Port"].get<std::uint16_t>());

			if (j.contains("HotLoad") && j["HotLoad"].is_boolean()) {
				cfg.SetHotLoad(j["HotLoad"]);
			}
			else {
				cfg.SetHotLoad(false);
			}

			ParseMqtt(j, cfg);
			ParseDatabase(j, cfg);
			ParseLogging(j, cfg);
		} catch(json::exception &ex) {
			std::cerr << "Unable to parse configuration file: " <<
			          ex.what() << std::endl;
			throw;
		}

		return cfg;
	}
}
