/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/logging.h>

namespace sensateiot::config
{
	const std::string& Logging::GetLevel() const
	{
		return this->m_level;
	}

	const std::string &Logging::GetPath() const
	{
		return this->m_path;
	}

	void Logging::SetPath(const std::string &path)
	{
		this->m_path = path;
	}

	void Logging::SetLevel(const std::string &level)
	{
		this->m_level = level;
	}
}
