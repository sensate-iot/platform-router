/*
 * Sensate IoT application class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/application.h>
#include <sensateiot/configparser.h>
#include <sensateiot/version.h>


#include <json.hpp>
#include <google/protobuf/any.h>

#include <boost/uuid/uuid_io.hpp>
#include <boost/lexical_cast.hpp>

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

	void Application::Startup()
	{
		this->m_config = parser::Parse(this->m_configPath);
		util::Log::StartLogging(this->m_config.GetLogging());
		auto &log = util::Log::GetLog();

		log << "Starting Sensate IoT AuthService " << version::VersionString << util::Log::NewLine;
		util::MongoDBClientPool::Init(this->m_config.GetDatabase().GetMongoDB());
		this->m_commands.reset(new consumers::CommandConsumer());

		// Internal client
		mqtt::MqttInternalCallback icb(this->m_config, *this->m_commands);
		auto ihost = this->m_config.GetMqtt().GetPrivateBroker().GetBroker().GetUri();
		this->m_client.reset(new mqtt::InternalMqttClient(
			ihost,
			this->m_config.GetMqtt().GetPrivateBroker().GetClientId(),
			std::move(icb)
		));
		this->m_client->Connect(this->m_config.GetMqtt());

		this->m_apiKeyRepository.reset(new services::ApiKeyRepository(this->m_config.GetDatabase().GetPostgreSQL()));
		this->m_userRepository.reset(new services::UserRepository(this->m_config.GetDatabase().GetPostgreSQL()));
		this->m_sensorRepository.reset(new services::SensorRepository(this->m_config.GetDatabase().GetMongoDB()));
		this->m_msgService.reset(
			new services::MessageService(
				*this->m_client, 
				*this->m_commands, 
				*this->m_userRepository, 
				*this->m_apiKeyRepository, 
				*this->m_sensorRepository,
				this->m_config)
		);

		this->m_addKeyHandler.reset(new commands::AddKeyCommandHandler(*this->m_msgService));
		this->m_addUserHandler.reset(new commands::AddUserCommandHandler(*this->m_msgService));
		this->m_addSensorHandler.reset(new commands::AddSensorCommandHandler(*this->m_msgService));
		
		this->m_flushKeyHandler.reset(new commands::FlushKeyCommandHandler(*this->m_msgService));
		this->m_flushUserHandler.reset(new commands::FlushUserCommandHandler(*this->m_msgService));
		this->m_flushSensorHandler.reset(new commands::FlushSensorCommandHandler(*this->m_msgService));

		this->m_commands->AddHandler(FlushSensorCmd.data(), *this->m_flushSensorHandler);
		this->m_commands->AddHandler(FlushKeyCmd.data(), *this->m_flushKeyHandler);
		this->m_commands->AddHandler(FlushUserCmd.data(), *this->m_flushUserHandler);
		this->m_commands->AddHandler(AddUserCmd.data(), *this->m_addUserHandler);
		this->m_commands->AddHandler(AddSensorCmd.data(), *this->m_addSensorHandler);
		this->m_commands->AddHandler(AddKeyCmd.data(), *this->m_addKeyHandler);

		this->m_msgService->LoadAll();

		this->m_httpServer.reset(new httpd::HttpServer(this->m_config));
		this->m_statusHandler.reset(new http::StatusHandler());
		this->m_bulkMeasurementHandler.reset(new http::BulkMeasurementHandler(*this->m_msgService));
		this->m_bulkMessageHandler.reset(new http::BulkMessageHandler(*this->m_msgService));
		this->m_measurementHandler.reset(new http::MeasurementHandler(*this->m_msgService));
		this->m_messageHandler.reset(new http::MessageHandler(*this->m_msgService));
		
		this->m_httpServer->AddHandler("/authorization/v1/status", *this->m_statusHandler);
		this->m_httpServer->AddHandler("/authorization/v1/processor/message", *this->m_messageHandler);
		this->m_httpServer->AddHandler("/authorization/v1/processor/messages", *this->m_bulkMessageHandler);
		this->m_httpServer->AddHandler("/authorization/v1/processor/measurements", *this->m_bulkMeasurementHandler);
		this->m_httpServer->AddHandler("/authorization/v1/processor/measurement", *this->m_measurementHandler);
	}

	void Application::Run()
	{
		std::atomic_bool done = false;
		auto &log = util::Log::GetLog();
		
		log << "AuthService started!" << util::Log::NewLine;

		std::thread runner([&]() {
			try {
				while (!done) {
					auto time = this->m_msgService->Process();
					time_t interval = this->m_config.GetInterval();

					if (time < interval) {
						std::this_thread::sleep_for(std::chrono::milliseconds(interval - time));
					}
				}
			} catch (std::exception& ex) {
				util::Log::GetLog() << "Unable to process messages: " << ex.what() << util::Log::NewLine;
			}
		});

		this->m_httpServer->Run();

		done = true;
		log << "Stopping AuthService..." << util::Log::NewLine;
		runner.join();
		log << "AuthService stopped." << util::Log::NewLine;
	}
}

void CreateApplication(const char *path)
{
	try {
		sensateiot::Application app;
		//auto &app = sensateiot::Application::GetApplication();
		app.SetConfig(path);

		app.Startup();
		app.Run();
	} catch(mqtt::exception& ex) {
		std::cerr << "Unable to run application: " << ex.printable_error(ex.get_return_code(), ex.get_reason_code()) << std::endl;
		std::cerr << "Exception message: " << ex.what() << std::endl;
	} catch(std::runtime_error &ex) {
		std::cerr << "Unable to run application: " << ex.what();
	} catch(std::exception &ex) {
		std::cerr << "Unable to run application: " << ex.what();
	}
}
