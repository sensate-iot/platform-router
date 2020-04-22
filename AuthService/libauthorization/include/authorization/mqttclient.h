/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/mqtt.h>

#include <mqtt/client.h>
#include <mqtt/async_client.h>

namespace sensateiot::auth
{
	class DLL_EXPORT MqttClient {
	public:
		explicit MqttClient(const std::string& uri, bool internal);
		virtual ~MqttClient();

		MqttClient(MqttClient&& rhs) noexcept = delete ;
		MqttClient& operator=(MqttClient&& rhs) noexcept = delete;

		MqttClient(const MqttClient&) = delete;
		MqttClient& operator=(const MqttClient&) = delete;

		void connect(const config::Mqtt& config);

	private:
		mqtt::async_client m_client;
		bool m_internal;
	};
}
