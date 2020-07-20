/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <sensateiot/util/log.h>
#include <sensateiot/mqtt/mqttclient.h>

#include <iostream>

namespace sensateiot::mqtt
{
	void BaseMqttClient::Connect(const config::Mqtt &config)
	{
		try {
			auto& log = util::Log::GetLog();
			log << "Connecting to MQTT broker: " << this->m_client.get_server_uri() << util::Log::NewLine;

			auto token = this->m_client.connect(this->m_opts);
			token->wait();
		} catch(const ::mqtt::exception& ex) {
			std::cerr << "Unable to connect MQTT client: " <<
			          ex.what() << std::endl;
		}
	}
	
	void BaseMqttClient::Connect(const config::Mqtt &config, const ns_base::mqtt::connect_options& opts)
	{
		try {
			auto& log = util::Log::GetLog();
			log << "Connecting to MQTT broker: " << this->m_client.get_server_uri() << util::Log::NewLine;

			auto token = this->m_client.connect();
			token->wait();
		} catch(const ::mqtt::exception& ex) {
			std::cerr << "Unable to connect MQTT client: " <<
			          ex.what() << std::endl;
		}
	}

	ns_base::mqtt::delivery_token_ptr BaseMqttClient::Publish(const std::string& topic, const std::string& msg)
	{
		return this->m_client.publish(topic, msg);
		//result->wait();
	}

	BaseMqttClient::BaseMqttClient(const std::string &host, const std::string &id) :
		m_client(host, id)
	{
		this->m_opts.set_automatic_reconnect(true);
		this->m_opts.set_clean_session(true);
		this->m_opts.set_keep_alive_interval(20);
	}

	BaseMqttClient::~BaseMqttClient()
	{
		try {
			this->m_client.disconnect()->wait();
		} catch(const ::mqtt::exception& ex) {
			std::cerr << "Unable to close MQTT client: ";
			std::cerr << ex.what() << std::endl;
		}
	}

	void BaseMqttClient::SetCallback(ns_base::mqtt::callback& cb)
	{
		this->m_client.set_callback(cb);
	}

	InternalMqttClient::InternalMqttClient(const std::string& host, const std::string& id, MqttInternalCallback cb) :
		BaseMqttClient(host, id), m_cb(std::move(cb))
	{
		this->m_cb.set_client(this->m_client, this->m_opts);
	}

	void InternalMqttClient::Connect(const config::Mqtt &config)
	{
		this->SetCallback(this->m_cb);

		this->m_opts.set_user_name(config.GetPrivateBroker().GetBroker().GetUsername());
		this->m_opts.set_password(config.GetPrivateBroker().GetBroker().GetPassword());
		BaseMqttClient::Connect(config);
	}
}
