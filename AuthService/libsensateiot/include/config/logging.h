/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <string>

namespace sensateiot::config
{
	class DLL_EXPORT Logging {
	public:
		const std::string& GetPath() const;
		const std::string& GetLevel() const;

		void SetPath(const std::string& path);
		void SetLevel(const std::string& level);

	private:
		std::string m_path;
		std::string m_level;
	};
}
