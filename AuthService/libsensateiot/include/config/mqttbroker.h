/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <cstdint>
#include <string>

namespace sensateiot::config
{
	class DLL_EXPORT MqttBroker {
	public:
		[[nodiscard]]
		const std::string& GetHostName() const;
		void SetHostName(const std::string& host);

		[[nodiscard]]
		const std::string& GetUsername() const;
		void SetUsername(const std::string& username);

		[[nodiscard]]
		const std::string& GetPassword() const;
		void SetPassword(const std::string& password);

		[[nodiscard]]
		std::uint16_t GetPort() const;
		void SetPortNumber(std::uint16_t port);

		[[nodiscard]]
		bool GetSsl() const;
		void SetSsl(bool enable);

		[[nodiscard]]
		std::string GetUri() const;

	private:
		std::string m_host;
		std::string m_username;
		std::string m_password;
		std::uint16_t m_port{};
		bool m_ssl{};
	};

	class DLL_EXPORT PrivateBroker {
	public:
		[[nodiscard]]
		const MqttBroker& GetBroker() const;
		MqttBroker& GetBroker();

		[[nodiscard]]
		const std::string& GetBulkMessageTopic() const;
		void SetBulkMessageTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetBulkMeasurementTopic() const;
		void SetBulkMeasurementTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetCommandTopic() const;
		void SetCommandTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetClientId() const;
		void SetClientId(const std::string& id);

	private:
		MqttBroker m_broker{};
		std::string m_internalBulkMeasurementTopic;
		std::string m_internalBulkMessageTopic;
		std::string m_clientId;
		std::string m_commandTopic;
	};
}
