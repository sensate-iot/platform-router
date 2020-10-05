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

		[[nodiscard]]
		std::uint16_t GetHttpPort() const;
		void SetHttpPort(std::uint16_t port);

		[[nodiscard]]
		const std::string& GetBindAddress() const;
		void SetBindAddress(const std::string& addr);

		Logging& GetLogging();

	private:
		Database m_db{};
		Mqtt m_mqtt{};
		Logging m_logging{};
		int m_workers{};
		int m_interval{};
		std::size_t m_internalBatchSize{};
		std::string m_bindAddr;
		std::uint16_t m_port{};
	};
}
