/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/config.h>

namespace sensateiot::config
{

	Database& Config::GetDatabase()
	{
		return this->m_db;
	}

	Mqtt& Config::GetMqtt()
	{
		return this->m_mqtt;
	}

	void Config::SetWorkers(int workers)
	{
		this->m_workers = workers;
	}

	int Config::GetWorkers() const
	{
		return this->m_workers;
	}

	void Config::SetInterval(int interval)
	{
		this->m_interval = interval;
	}

	int Config::GetInterval() const
	{
		return this->m_interval;
	}

	Logging &Config::GetLogging()
	{
		return this->m_logging;
	}
}
