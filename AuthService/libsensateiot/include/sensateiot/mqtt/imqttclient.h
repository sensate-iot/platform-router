/*
 * MQTT client interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/mqtt.h>
#include <mqtt/async_client.h>

#include <string>

namespace sensateiot::mqtt
{
	class IMqttClient {
	public:
		virtual ~IMqttClient() = default;
		virtual void Connect(const config::Mqtt&) = 0;
		virtual ns_base::mqtt::delivery_token_ptr Publish(const std::string& topic, const std::string& msg) = 0;
	};
}
