/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#pragma once

#include <authorization/mqttclient.h>

#include <mqtt/async_client.h>

namespace sensateiot::auth::detail
{
	class MqttCallback :
			public mqtt::callback,
			public virtual mqtt::iaction_listener {
	public:
		explicit MqttCallback(mqtt::async_client& cli, mqtt::connect_options& connOpts);

		void on_failure(const mqtt::token& tok) override;
		void delivery_complete(mqtt::delivery_token_ptr token) override;
		void on_success(const mqtt::token& tok) override;
		void connected(const std::string& cause) override;
		void connection_lost(const std::string& cause) override;
		void message_arrived(mqtt::const_message_ptr msg) override;

	private:
		mqtt::async_client& m_cli;
		mqtt::connect_options& m_connOpts;
		int m_retry;
	};

	class InternalMqttCallback {

	};
}
