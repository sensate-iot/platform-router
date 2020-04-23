/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <authorization/mqttcallback.h>
#include <authorization/mqttclient.h>

#include <iostream>

namespace sensateiot::mqtt
{
	void BaseMqttClient::Connect(const config::Mqtt &config)
	{
		try {
			std::cout << "Connecting..." << std::endl;
			auto token = this->m_client.connect();
			token->wait();
		} catch(const ::mqtt::exception& ex) {
			std::cerr << "Unable to connect MQTT client: " <<
			          ex.what() << std::endl;
		}
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

	void BaseMqttClient::SetCallback(::mqtt::callback& cb)
	{
		this->m_client.set_callback(cb);
	}

	MqttClient::MqttClient(const std::string &host, const std::string &id, MqttCallback cb) :
		BaseMqttClient(host, id), m_cb(std::move(cb))
	{
	}

	void MqttClient::Connect(const config::Mqtt &config)
	{
		this->SetCallback(this->m_cb);
		BaseMqttClient::Connect(config);
	}
}
