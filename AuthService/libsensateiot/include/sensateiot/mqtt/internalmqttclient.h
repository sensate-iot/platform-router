/*
 * Sensate IoT MQTT client.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

#pragma once

#include <mqtt/client.h>
#include <mqtt/async_client.h>

#include <sensateiot/mqtt/basemqttclient.h>
#include <sensateiot/mqtt/imqttclient.h>

namespace sensateiot::mqtt
{
	class DLL_EXPORT InternalMqttClient : public BaseMqttClient {
	public:
		explicit InternalMqttClient(const std::string& host, const std::string& id, MqttInternalCallback cb);
		~InternalMqttClient() override = default;
		void Connect(const config::Mqtt &config) override;

	private:
		MqttInternalCallback m_cb;
		stl::ReferenceWrapper<consumers::CommandConsumer> m_commands;
	};
}
