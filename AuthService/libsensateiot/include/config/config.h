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
#include <config/logging.h>

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

		[[nodiscard]]
		std::size_t GetInternalBatchSize() const;
		void SetInternalBatchSize(std::size_t size);

		Logging& GetLogging();

	private:
		Database m_db{};
		Mqtt m_mqtt{};
		Logging m_logging{};
		int m_workers{};
		int m_interval{};
		std::size_t m_internalBatchSize{};
	};
}
