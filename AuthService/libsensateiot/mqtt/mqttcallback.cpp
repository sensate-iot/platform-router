/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <sensateiot/mqtt/mqttcallback.h>

#include <iostream>

namespace sensateiot::mqtt
{
	MqttCallback::MqttCallback(ns_base::mqtt::async_client& cli, ns_base::mqtt::connect_options& connOpts)
		: m_cli(cli), m_connOpts(connOpts)
	{

	}

	void MqttCallback::on_failure(const ns_base::mqtt::token &tok)
	{
		auto& log = util::Log::GetLog();
		log << "MQTT client failure!" << util::Log::NewLine;
	}

	void MqttCallback::delivery_complete(ns_base::mqtt::delivery_token_ptr token)
	{
	}

	void MqttCallback::on_success(const ns_base::mqtt::token &tok)
	{
		auto& log = util::Log::GetLog();

		if(tok.get_type() == ns_base::mqtt::token::Type::SUBSCRIBE) {
			log << "Subscribed!" << util::Log::NewLine;
		}
	}

	void MqttCallback::connected(const std::string &cause)
	{
		auto& log = util::Log::GetLog();

		log << "MQTT client connected!" << util::Log::NewLine;
		log << "Subscribing to: " << this->m_config.GetPublicBroker().GetMeasurementTopic() << util::Log::NewLine;
		log << "Subscribing to: " << this->m_config.GetPublicBroker().GetBulkMeasurementTopic() << util::Log::NewLine;
		log << "Subscribing to: " << this->m_config.GetPublicBroker().GetMessageTopic() << util::Log::NewLine;

		this->m_cli->subscribe(
				this->m_config.GetPublicBroker().GetMeasurementTopic(),
				0 );
		this->m_cli->subscribe(
				this->m_config.GetPublicBroker().GetBulkMeasurementTopic(),
				0 );
		this->m_cli->subscribe(
				this->m_config.GetPublicBroker().GetMessageTopic(),
				0 );
	}

	void MqttCallback::connection_lost(const std::string &cause)
	{
		auto& log = util::Log::GetLog();
		log << "MQTT connection lost!" << util::Log::NewLine;
	}

	void MqttCallback::message_arrived(ns_base::mqtt::const_message_ptr msg)
	{
		auto& log = util::Log::GetLog();
		log << "Got message on: " << msg->get_topic() << util::Log::NewLine;
	}

	void MqttCallback::set_config(const config::Mqtt &mqtt)
	{
		this->m_config = mqtt;
	}

	void MqttCallback::set_client(ns_base::mqtt::async_client &cli, ns_base::mqtt::connect_options &opts)
	{
		this->m_cli = cli;
		this->m_connOpts = opts;
	}
}
