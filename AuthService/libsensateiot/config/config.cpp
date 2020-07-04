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

	std::size_t Config::GetInternalBatchSize() const
	{
		return this->m_internalBatchSize;
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

	void Config::SetInternalBatchSize(std::size_t size)
	{
		this->m_internalBatchSize = size;
	}

	Logging &Config::GetLogging()
	{
		return this->m_logging;
	}
}
