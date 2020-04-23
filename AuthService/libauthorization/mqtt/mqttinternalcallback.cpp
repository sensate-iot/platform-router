/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <sensateiot/mqttinternalcallback.h>
#include <iostream>

namespace sensateiot::mqtt
{
	MqttInternalCallback::MqttInternalCallback(
			ns_base::mqtt::async_client& cli,
			ns_base::mqtt::connect_options& connOpts
	) : m_retry(0), m_cli(cli), m_connOpts(connOpts)
	{

	}

	void MqttInternalCallback::on_failure(const ::mqtt::token &tok)
	{
		std::cout << "MQTT client failure!" << std::endl;
	}

	void MqttInternalCallback::delivery_complete(::mqtt::delivery_token_ptr token)
	{
		callback::delivery_complete(token);
	}

	void MqttInternalCallback::on_success(const ::mqtt::token &tok)
	{
	}

	void MqttInternalCallback::connected(const std::string &cause)
	{
		std::cout << "MQTT client connected!" << std::endl;
	}

	void MqttInternalCallback::connection_lost(const std::string &cause)
	{
		std::cout << "MQTT connection lost!" << std::endl;
	}

	void MqttInternalCallback::message_arrived(::mqtt::const_message_ptr msg)
	{
		callback::message_arrived(msg);
	}
}
