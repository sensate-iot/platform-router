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

#include <sensateiot/mqtt/mqttinternalcallback.h>
#include <sensateiot/mqtt/imqttclient.h>

namespace sensateiot::mqtt
{
	class BaseMqttClient : public IMqttClient {
	public:
		BaseMqttClient(const std::string& host, const std::string& id);
		~BaseMqttClient() override;

		BaseMqttClient(BaseMqttClient&& rhs) noexcept = delete ;
		BaseMqttClient& operator=(BaseMqttClient&& rhs) noexcept = delete;

		BaseMqttClient(const BaseMqttClient&) = delete;
		BaseMqttClient& operator=(const BaseMqttClient&) = delete;

		void Connect(const config::Mqtt &config, const ns_base::mqtt::connect_options& opts);
		void Connect(const config::Mqtt &config) override;
		void Publish(const std::string& topic, const std::string& msg) override;

	protected:
		void SetCallback(::mqtt::callback& cb);

		ns_base::mqtt::async_client m_client;
		ns_base::mqtt::connect_options m_opts;
	};

	class DLL_EXPORT InternalMqttClient : public BaseMqttClient {
	public:
		explicit InternalMqttClient(const std::string& host, const std::string& id, MqttInternalCallback cb);
		void Connect(const config::Mqtt &config) override;

	private:
		MqttInternalCallback m_cb;
	};
}
