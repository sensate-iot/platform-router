/*
 * Sensate IoT application class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef __cplusplus

#include <string>
#include <config/config.h>

#include <json.hpp>

extern "C" void CreateApplication(const char*);

namespace sensateiot::auth
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
	};
}
#else

extern void CreateApplication(const char*);

#endif
