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

#include <string>
#include <json.hpp>
#include <string_view>

extern "C" void CreateApplication(const char*);

namespace sensateiot
{
	class Application {
	public:
		static Application &GetApplication()
		{
			static Application app;
			return app;
		}

		config::Config &GetConfig();
		void SetConfig(std::string path);
		void Run();

	private:
		config::Config m_config;
		std::string m_configPath;

		explicit Application() = default;

		void ParseConfig();
		void ParseMqtt(nlohmann::json& json);
		void ParseDatabase(nlohmann::json&  json);
		void ParseLogging(nlohmann::json& json);

		static constexpr auto FlushKeyCmd = std::string_view("flush_key");
		static constexpr auto FlushSensorCmd = std::string_view("flush_sensor");
		static constexpr auto FlushUserCmd = std::string_view("flush_user");
	};
}
#else

extern void CreateApplication(const char*);

#endif
