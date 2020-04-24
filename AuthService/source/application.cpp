/*
 * Sensate IoT application class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/application.h>
#include <sensateiot/mqttclient.h>
#include <sensateiot/messageservice.h>

#include <json.hpp>

#include <fstream>
#include <iostream>

namespace sensateiot
{
	config::Config &Application::GetConfig()
	{
		return this->m_config;
	}

	void Application::SetConfig(std::string path)
	{
		this->m_configPath = std::move(path);
	}

	void Application::Run()
	{
		this->ParseConfig();
		util::Log::StartLogging(this->m_config.GetLogging());

		auto& log = util::Log::GetLog();
		log << "Starting Sensate IoT AuthService..." << util::Log::NewLine;

//		std::string hostname = this->m_config.GetMqtt().GetPublicBroker().GetBroker().GetHostName();
//		hostname += ":" + std::to_string(this->m_config.GetMqtt().GetPublicBroker().GetBroker().GetPort());
		auto hostname = this->m_config.GetMqtt().GetPublicBroker().GetBroker().GetUri();

		mqtt::MqttCallback cb;
		mqtt::MqttClient client(hostname, "a23fa-badf", std::move(cb));
		client.Connect(this->m_config.GetMqtt());

		// Internal client
		mqtt::MqttInternalCallback icb;
		auto ihost = this->m_config.GetMqtt().GetPublicBroker().GetBroker().GetUri();
		mqtt::InternalMqttClient iclient(ihost, "3lasdfjlas", std::move(icb));
		iclient.Connect(this->m_config.GetMqtt());

		mqtt::MessageService service(iclient, this->m_config);

		std::cin.get();

		// TODO: run queue timer
	}

	void Application::ParseConfig()
	{
		using namespace nlohmann;
		std::ifstream file(this->m_configPath);

		if(!file.good()) {
			throw std::runtime_error("Config file not found!");
		}

		std::string content(
				(std::istreambuf_iterator<char>(file)),
				        std::istreambuf_iterator<char>());

		try {
			auto j = json::parse(content);

			this->ParseMqtt(j);
			this->ParseDatabase(j);
			this->ParseLogging(j);
		} catch(json::exception& ex) {
			std::cerr << "Unable to parse configuration file: " <<
				ex.what() << std::endl;
		}
	}

	void Application::ParseMqtt(nlohmann::json& j)
	{
		this->m_config.SetInterval(j["Interval"]);
		this->m_config.SetWorkers(j["Workers"]);

		this->m_config.GetMqtt().GetPrivateBroker()
				.GetBroker().SetHostName(j["Mqtt"]["InternalBroker"]["Host"]);
		this->m_config.GetMqtt().GetPrivateBroker()
				.GetBroker().SetPort(j["Mqtt"]["InternalBroker"]["Port"]);
		this->m_config.GetMqtt().GetPrivateBroker()
				.GetBroker().SetUsername(j["Mqtt"]["InternalBroker"]["Username"]);
		this->m_config.GetMqtt().GetPrivateBroker()
				.GetBroker().SetPassword(j["Mqtt"]["InternalBroker"]["Password"]);
		this->m_config.GetMqtt().GetPrivateBroker()
				.GetBroker().SetSsl(j["Mqtt"]["InternalBroker"]["Ssl"] == "true");
		this->m_config.GetMqtt().GetPrivateBroker()
				.SetBulkMeasurementTopic(j["Mqtt"]["InternalBroker"]["InternalBulkMeasurementTopic"]);
		this->m_config.GetMqtt().GetPrivateBroker()
				.SetMeasurementTopic(j["Mqtt"]["InternalBroker"]["InternalMeasurementTopic"]);
		this->m_config.GetMqtt().GetPrivateBroker()
				.SetMessageTopic(j["Mqtt"]["InternalBroker"]["InternalMessageTopic"]);

		this->m_config.GetMqtt().GetPublicBroker()
				.GetBroker().SetHostName(j["Mqtt"]["PublicBroker"]["Host"]);
		this->m_config.GetMqtt().GetPublicBroker()
				.GetBroker().SetPort(j["Mqtt"]["PublicBroker"]["Port"]);
		this->m_config.GetMqtt().GetPublicBroker()
				.GetBroker().SetUsername(j["Mqtt"]["PublicBroker"]["Username"]);
		this->m_config.GetMqtt().GetPublicBroker()
				.GetBroker().SetPassword(j["Mqtt"]["PublicBroker"]["Password"]);
		this->m_config.GetMqtt().GetPublicBroker()
				.GetBroker().SetSsl(j["Mqtt"]["PublicBroker"]["Ssl"] == "true");
		this->m_config.GetMqtt().GetPublicBroker()
				.SetBulkMeasurementTopic(j["Mqtt"]["PublicBroker"]["BulkMeasurementTopic"]);
		this->m_config.GetMqtt().GetPublicBroker()
				.SetMeasurementTopic(j["Mqtt"]["PublicBroker"]["MeasurementTopic"]);
		this->m_config.GetMqtt().GetPublicBroker()
				.SetMessageTopic(j["Mqtt"]["PublicBroker"]["MessageTopic"]);
	}

	void Application::ParseDatabase(nlohmann::json &json)
	{
		this->m_config.GetDatabase().GetPostgreSQL()
				.SetConnectionString(json["Database"]["PgSQL"]["ConnectionString"]);
		this->m_config.GetDatabase().GetMongoDB()
				.SetDatabaseName(json["Database"]["MongoDB"]["DatabaseName"]);
		this->m_config.GetDatabase().GetPostgreSQL()
				.SetConnectionString(json["Database"]["MongoDB"]["ConnectionString"]);
	}

	void Application::ParseLogging(nlohmann::json& json)
	{
		this->m_config.GetLogging().SetLevel(json["Logging"]["Level"]);
		this->m_config.GetLogging().SetPath(json["Logging"]["File"]);
	}
}

void CreateApplication(const char *path)
{
	auto& app = sensateiot::Application::GetApplication();

	try {
		app.SetConfig(path);
		app.Run();
	} catch(std::runtime_error& ex) {
		std::cerr << "Unable to run application: " << ex.what();
	} catch(std::exception& ex) {
		std::cerr << "Unable to run application: " << ex.what();
	}
}
