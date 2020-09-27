/*
 * Sensate IoT application class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef __cplusplus

#include <config/config.h>

#include <sensateiot/util/log.h>
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

#include <sensateiot/commands/addsensorcommandhandler.h>
#include <sensateiot/commands/addkeycommandhandler.h>
#include <sensateiot/commands/addusercommandhandler.h>

#include <string>
#include <json.hpp>
#include <string_view>
#include <memory>

extern "C" void CreateApplication(const char*);

namespace sensateiot
{
	class Application {
	public:
		explicit Application() = default;
		~Application();
		
		config::Config &GetConfig();
		void SetConfig(std::string path);
		void Startup();
		void Run();

	private:
		config::Config m_config;
		std::string m_configPath;

		std::unique_ptr<services::ApiKeyRepository> m_apiKeyRepository;
		std::unique_ptr<services::UserRepository> m_userRepository;
		std::unique_ptr<services::SensorRepository> m_sensorRepository;
		std::unique_ptr<services::MessageService> m_msgService;
		std::unique_ptr<mqtt::InternalMqttClient> m_client;

		std::unique_ptr<consumers::CommandConsumer> m_commands;
		std::unique_ptr<commands::AddKeyCommandHandler> m_addKeyHandler;
		std::unique_ptr<commands::AddSensorCommandHandler> m_addSensorHandler;
		std::unique_ptr<commands::AddUserCommandHandler> m_addUserHandler;
		std::unique_ptr<commands::FlushUserCommandHandler> m_flushUserHandler;
		std::unique_ptr<commands::FlushKeyCommandHandler> m_flushKeyHandler;
		std::unique_ptr<commands::FlushSensorCommandHandler> m_flushSensorHandler;

		std::unique_ptr<httpd::HttpServer> m_httpServer;
		std::unique_ptr<http::StatusHandler> m_statusHandler;
		std::unique_ptr<http::BulkMeasurementHandler> m_bulkMeasurementHandler;
		std::unique_ptr<http::BulkMessageHandler> m_bulkMessageHandler;
		std::unique_ptr<http::MeasurementHandler> m_measurementHandler;
		std::unique_ptr<http::MessageHandler> m_messageHandler;


		static constexpr auto FlushKeyCmd = std::string_view("flush_key");
		static constexpr auto FlushSensorCmd = std::string_view("flush_sensor");
		static constexpr auto FlushUserCmd = std::string_view("flush_user");
		
		static constexpr auto AddKeyCmd = std::string_view("add_key");
		static constexpr auto AddSensorCmd = std::string_view("add_sensor");
		static constexpr auto AddUserCmd = std::string_view("add_user");
		
		friend void ::CreateApplication(const char* path);
	};
}
#else

extern void CreateApplication(const char*);

#endif
