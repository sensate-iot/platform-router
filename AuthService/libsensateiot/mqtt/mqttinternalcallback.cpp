/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <sensateiot/mqttinternalcallback.h>
#include <sensateiot/log.h>

#include <iostream>

namespace sensateiot::mqtt
{
	MqttInternalCallback::MqttInternalCallback(
			ns_base::mqtt::async_client& cli,
			ns_base::mqtt::connect_options& connOpts
	) : m_cli(cli), m_connOpts(connOpts)
	{

	}

	void MqttInternalCallback::on_failure(const ::mqtt::token &tok)
	{
		auto& log = util::Log::GetLog();
		log << "Internal MQTT client failure!" << util::Log::NewLine;
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
		auto& log = util::Log::GetLog();
		log << "Internal MQTT client connected!" << util::Log::NewLine;
	}

	void MqttInternalCallback::connection_lost(const std::string &cause)
	{
		auto& log = util::Log::GetLog();
		log << "Internal MQTT client connection lost!" << util::Log::NewLine;
	}

	void MqttInternalCallback::message_arrived(::mqtt::const_message_ptr msg)
	{
		callback::message_arrived(msg);
	}

	void MqttInternalCallback::set_client(ns_base::mqtt::async_client &cli, ns_base::mqtt::connect_options &opts)
	{
		this->m_cli = cli;
		this->m_connOpts = opts;
	}
}
