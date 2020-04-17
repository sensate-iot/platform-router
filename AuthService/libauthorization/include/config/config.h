/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <config/database.h>
#include <config/mqtt.h>

namespace sensateiot::auth::config
{
	class Config {
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
