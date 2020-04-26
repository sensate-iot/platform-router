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
		void SetPort(std::uint16_t port);

		[[nodiscard]]
		bool GetSsl() const;
		void SetSsl(bool enable);

		[[nodiscard]]
		std::string GetUri() const;

	private:
		std::string m_host;
		std::string m_username;
		std::string m_password;
		std::uint16_t m_port;
		bool m_ssl;
	};

	class DLL_EXPORT PublicBroker {
	public:
		MqttBroker& GetBroker() ;

		[[nodiscard]]
		const std::string& GetMeasurementTopic() const;
		void SetMeasurementTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetMessageTopic() const;
		void SetMessageTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetBulkMeasurementTopic() const;
		void SetBulkMeasurementTopic(const std::string& topic);

	private:
		MqttBroker m_broker;
		std::string m_measurementTopic;
		std::string m_bulkMeasurementTopic;
		std::string m_messageTopic;
	};

	class DLL_EXPORT PrivateBroker {
	public:
		MqttBroker& GetBroker() ;

		[[nodiscard]]
		const std::string& GetMeasurementTopic() const;
		void SetMeasurementTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetMessageTopic() const;
		void SetMessageTopic(const std::string& topic);

		[[nodiscard]]
		const std::string& GetBulkMeasurementTopic() const;
		void SetBulkMeasurementTopic(const std::string& topic);

	private:
		MqttBroker m_broker;
		std::string m_internalMeasurementTopic;
		std::string m_internalBulkMeasurementTopic;
		std::string m_internalMessageTopic;
	};
}
