/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <mqtt/async_client.h>

namespace sensateiot::mqtt
{
	class DLL_EXPORT MqttInternalCallback :
				public ::mqtt::callback,
				public virtual ::mqtt::iaction_listener {
	public:
		MqttInternalCallback(::mqtt::async_client& cli, ::mqtt::connect_options& connOpts);

		void on_failure(const ::mqtt::token& tok) override;
		void delivery_complete(::mqtt::delivery_token_ptr token) override;
		void on_success(const ::mqtt::token& tok) override;
		void connected(const std::string& cause) override;
		void connection_lost(const std::string& cause) override;
		void message_arrived(::mqtt::const_message_ptr msg) override;

	private:
		std::reference_wrapper<::mqtt::async_client> m_cli;
		std::reference_wrapper<::mqtt::connect_options> m_connOpts;
	};
}
