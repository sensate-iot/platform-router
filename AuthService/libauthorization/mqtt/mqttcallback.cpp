/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <authorization/mqttcallback.h>
#include <iostream>

namespace sensateiot::mqtt
{
	MqttCallback::MqttCallback(::mqtt::async_client& cli, ::mqtt::connect_options& connOpts)
		: m_cli(cli), m_connOpts(connOpts)
	{

	}

	void MqttCallback::on_failure(const ::mqtt::token &tok)
	{
		std::cout << "MQTT client failure!" << std::endl;
	}

	void MqttCallback::delivery_complete(::mqtt::delivery_token_ptr token)
	{
		callback::delivery_complete(token);
	}

	void MqttCallback::on_success(const ::mqtt::token &tok)
	{
	}

	void MqttCallback::connected(const std::string &cause)
	{
		std::cout << "MQTT client connected!" << std::endl;
	}

	void MqttCallback::connection_lost(const std::string &cause)
	{
		std::cout << "MQTT connection lost!" << std::endl;
	}

	void MqttCallback::message_arrived(::mqtt::const_message_ptr msg)
	{
		callback::message_arrived(msg);
	}
}
