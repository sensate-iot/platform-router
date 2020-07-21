/*
 * Internal MQTT client implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/log.h>
#include <sensateiot/mqtt/basemqttclient.h>
#include <sensateiot/mqtt/internalmqttclient.h>

namespace sensateiot::mqtt
{
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
