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
		m_client(host, "1234"), m_internal(internal)
	{
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
			mqtt::connect_options opts;

			opts.set_keep_alive_interval(20);
			opts.set_clean_session(true);
			opts.set_automatic_reconnect(true);

			detail::MqttCallback cb(this->m_client, opts);
			this->m_client.set_callback(cb);

			std::cout << "Connecting..." << std::endl;
			auto token = this->m_client.connect();
			token->wait();

		} catch(const mqtt::exception& ex) {
			std::cerr << "Unable to connect MQTT client: " <<
				ex.what() << std::endl;
		}
	}
}
