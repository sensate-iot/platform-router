/*
 * Logging stream.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <mutex>

#include <sensateiot.h>
#include <config/logging.h>

namespace sensateiot::util
{
	class DLL_EXPORT Log {
	private:
		explicit Log();

	public:
		struct DLL_EXPORT NewLineType {
			explicit constexpr NewLineType() = default;
		};

		static Log& GetLog();

		Log& operator<<(const std::string& input);
		Log& operator<<(NewLineType nl);

		void Flush();

		static void StartLogging(const config::Logging& logging);
		static constexpr NewLineType NewLine{};

	private:
		config::Logging m_config;
		std::string m_buffer;
		std::mutex m_lock;
	};
}

