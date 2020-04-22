/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <authorization/mqttcallback.h>
#include <mqtt/async_client.h>

#include <iostream>

namespace sensateiot::auth::detail
{
	MqttCallback::MqttCallback(mqtt::async_client& cli, mqtt::connect_options& connOpts)
		: m_retry(0), m_cli(cli), m_connOpts(connOpts)
	{

	}

	void MqttCallback::on_failure(const mqtt::token &tok)
	{
		std::cout << "MQTT client failure!" << std::endl;
	}

	void MqttCallback::delivery_complete(mqtt::delivery_token_ptr token)
	{
		callback::delivery_complete(token);
	}

	void MqttCallback::on_success(const mqtt::token &tok)
	{
	}

	void MqttCallback::connected(const std::string &cause)
	{
		std::cout << "MQTT client connected!" << std::endl;
//		callback::connected(cause);
	}

	void MqttCallback::connection_lost(const std::string &cause)
	{
		std::cout << "MQTT connection lost!" << std::endl;
//		callback::connection_lost(cause);
	}

	void MqttCallback::message_arrived(mqtt::const_message_ptr msg)
	{
		callback::message_arrived(msg);
	}
}
