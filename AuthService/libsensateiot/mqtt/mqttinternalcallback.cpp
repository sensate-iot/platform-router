/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <sensateiot/mqtt/mqttinternalcallback.h>
#include <sensateiot/util/log.h>
#include <sensateiot/consumers/commandconsumer.h>
#include <sensateiot/commands/command.h>
#include <sensateiot/stl/referencewrapper.h>

#include <iostream>

namespace sensateiot::mqtt
{
	MqttInternalCallback::MqttInternalCallback(
			ns_base::mqtt::async_client& cli,
			ns_base::mqtt::connect_options& connOpts,
			consumers::CommandConsumer& commands,
			const config::Config& cfg
	) : m_cli(cli), m_connOpts(connOpts), m_commands(commands), m_config(cfg)
	{
	}

	MqttInternalCallback::MqttInternalCallback(const config::Config& config, consumers::CommandConsumer& commands) : m_commands(commands), m_config(config)
	{
	}

	void MqttInternalCallback::on_failure(const ::mqtt::token &tok)
	{
		auto& log = util::Log::GetLog();
		log << "Internal MQTT client failure!" << util::Log::NewLine;

		this->reconnect();
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
		this->m_cli->subscribe(this->m_config.GetMqtt().GetPrivateBroker().GetCommandTopic(), QOS);
	}

	void MqttInternalCallback::connection_lost(const std::string &cause)
	{
		auto& log = util::Log::GetLog();
		log << "Internal MQTT client connection lost!" << util::Log::NewLine;

		this->reconnect();
	}

	void MqttInternalCallback::message_arrived(::mqtt::const_message_ptr msg)
	{
		callback::message_arrived(msg);

		if (msg->get_topic() == this->m_config.GetMqtt().GetPrivateBroker().GetCommandTopic()) {
			auto cmd = commands::Command::FromJson(msg->get_payload_str());

			if (!cmd.has_value()) {
				return;
			}

			this->m_commands->AddCommand(std::move(*cmd));
		}
	}

	void MqttInternalCallback::set_client(ns_base::mqtt::async_client &cli, ns_base::mqtt::connect_options &opts)
	{
		this->m_cli = cli;
		this->m_connOpts = opts;
	}

	void MqttInternalCallback::reconnect()
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(ReconnectTimeout));

		try {
			auto token = this->m_cli->connect(*this->m_connOpts, nullptr, *this);
		} catch(const ::mqtt::exception& ex) {
			auto& log = util::Log::GetLog();
			log << "Unable to reconnect: " << ex.what() << util::Log::NewLine;
		}
	}
}
