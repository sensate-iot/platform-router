/*
 * Sensate IoT application class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/application.h>
#include <sensateiot/configparser.h>

#include <sensateiot/consumers/commandconsumer.h>
#include <sensateiot/util/mongodbclientpool.h>

#include <sensateiot/httpd/httpserver.h>
#include <sensateiot/http/statushandler.h>
#include <sensateiot/http/bulkmeasurementhandler.h>
#include <sensateiot/http/measurementhandler.h>
#include <sensateiot/http/messagehandler.h>
#include <sensateiot/http/bulkmessagehandler.h>

#include <sensateiot/mqtt/basemqttclient.h>
#include <sensateiot/mqtt/internalmqttclient.h>

#include <sensateiot/services/userrepository.h>
#include <sensateiot/services/messageservice.h>
#include <sensateiot/services/apikeyrepository.h>
#include <sensateiot/services/sensorrepository.h>

#include <sensateiot/commands/flushsensorcommandhandler.h>
#include <sensateiot/commands/flushkeycommandhandler.h>
#include <sensateiot/commands/flushusercommandhandler.h>

#include <json.hpp>
#include <google/protobuf/any.h>

namespace sensateiot
{
	config::Config &Application::GetConfig()
	{
		return this->m_config;
	}

	Application::~Application()
	{
		util::MongoDBClientPool::Destroy();
		google::protobuf::ShutdownProtobufLibrary();
	}

	void Application::SetConfig(std::string path)
	{
		this->m_configPath = std::move(path);
	}

	void Application::Run()
	{
		std::atomic_bool done = false;
	
		this->m_config = parser::Parse(this->m_configPath);
		util::Log::StartLogging(this->m_config.GetLogging());
		auto &log = util::Log::GetLog();

		log << "Starting Sensate IoT AuthService..." << util::Log::NewLine;

		util::MongoDBClientPool::Init(this->m_config.GetDatabase().GetMongoDB());

		consumers::CommandConsumer commands;

		// Internal client
		mqtt::MqttInternalCallback icb(this->m_config, commands);
		auto ihost = this->m_config.GetMqtt().GetPrivateBroker().GetBroker().GetUri();
		mqtt::InternalMqttClient iclient(
			ihost,
			this->m_config.GetMqtt().GetPrivateBroker().GetClientId(),
			std::move(icb)
		);
		iclient.Connect(this->m_config.GetMqtt());

		services::UserRepository users(this->m_config.GetDatabase().GetPostgreSQL());
		services::ApiKeyRepository keys(this->m_config.GetDatabase().GetPostgreSQL());
		services::SensorRepository sensors(this->m_config.GetDatabase().GetMongoDB());
		services::MessageService service(iclient, commands, users, keys, sensors, this->m_config);

		commands::FlushSensorCommandHandler sensorHandler(service);
		commands::FlushKeyCommandHandler keyHandler(service);
		commands::FlushUserCommandHandler userHandler(service);

		commands.AddHandler(FlushSensorCmd.data(), sensorHandler);
		commands.AddHandler(FlushKeyCmd.data(), keyHandler);
		commands.AddHandler(FlushUserCmd.data(), userHandler);

		if (this->m_config.GetHotLoad()) {
			service.LoadAll();
		}

		log << "AuthService started!" << util::Log::NewLine;

		std::thread runner([&]() {
			while(!done) {
				auto time = service.Process();
				time_t interval = this->m_config.GetInterval();

				if(time < interval) {
					std::this_thread::sleep_for(std::chrono::milliseconds(interval - time));
				}
			}
		});

		httpd::HttpServer server(this->m_config);
		http::StatusHandler status;
		http::BulkMeasurementHandler bulkMeasurementHandler(service);
		http::MeasurementHandler measurementHandler(service);
		http::MessageHandler messageHandler(service);
		http::BulkMessageHandler bulkMessageHandler(service);

		server.AddHandler("/v1/status", status);
		server.AddHandler("/v1/processor/message", messageHandler);
		server.AddHandler("/v1/processor/messages", bulkMessageHandler);
		server.AddHandler("/v1/processor/measurements", bulkMeasurementHandler);
		server.AddHandler("/v1/processor/measurement", measurementHandler);
		server.Run();

		done = true;
		runner.join();
	}
}

void CreateApplication(const char *path)
{
	try {
		auto &app = sensateiot::Application::GetApplication();
		app.SetConfig(path);
		app.Run();
	} catch(std::runtime_error &ex) {
		std::cerr << "Unable to run application: " << ex.what();
	} catch(std::exception &ex) {
		std::cerr << "Unable to run application: " << ex.what();
	}
}
