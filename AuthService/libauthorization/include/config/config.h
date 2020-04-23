/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <config/database.h>
#include <config/mqtt.h>

namespace sensateiot::config
{
	class DLL_EXPORT Config {
	public:
		[[nodiscard]]
		Database& GetDatabase();

		[[nodiscard]]
		Mqtt& GetMqtt();

		[[nodiscard]]
		int GetInterval() const;
		void SetInterval(int interval);

		[[nodiscard]]
		int GetWorkers() const;
		void SetWorkers(int workers);

	private:
		Database m_db;
		Mqtt m_mqtt;
		int m_workers;
		int m_interval;
	};
}
