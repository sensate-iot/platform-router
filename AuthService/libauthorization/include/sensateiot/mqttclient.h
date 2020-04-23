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

#include <sensateiot/mqttcallback.h>

namespace sensateiot::mqtt
{
	class BaseMqttClient {
	public:
		BaseMqttClient(const std::string& host, const std::string& id);
		virtual ~BaseMqttClient();

		BaseMqttClient(BaseMqttClient&& rhs) noexcept = delete ;
		BaseMqttClient& operator=(BaseMqttClient&& rhs) noexcept = delete;

		BaseMqttClient(const BaseMqttClient&) = delete;
		BaseMqttClient& operator=(const BaseMqttClient&) = delete;

		virtual void Connect(const config::Mqtt &config);

	protected:
		void SetCallback(::mqtt::callback& cb);

		ns_base::mqtt::async_client m_client;
		ns_base::mqtt::connect_options m_opts;
	};

	class DLL_EXPORT MqttClient : public BaseMqttClient {
	public:
		MqttClient(const std::string& host, const std::string& id, MqttCallback cb);

		void Connect(const config::Mqtt &config) override;

	private:
		MqttCallback m_cb;
	};
}
