/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#include <authorization/mqttclient.h>
#include <authorization/mqttcallback.h>

namespace sensateiot::auth
{
	MqttClient::MqttClient(const std::string& host, bool internal) :
		m_client(host, "1lasdfjlasdfj234"), m_internal(internal),
		m_cb(m_client, m_opts)
	{
		this->m_client.set_callback(this->m_cb);
	}

	MqttClient::~MqttClient()
	{
		try {
			this->m_client.disconnect()->wait();
		} catch(const mqtt::exception& ex) {
			std::cerr << "Unable to close MQTT client: ";
			std::cerr << ex.what() << std::endl;
		}
	}

	void MqttClient::connect(const config::Mqtt &config)
	{
		try {
			std::cout << "Connecting..." << std::endl;
			auto token = this->m_client.connect();
			token->wait();
		} catch(const mqtt::exception& ex) {
			std::cerr << "Unable to connect MQTT client: " <<
				ex.what() << std::endl;
		}
	}
}
