/*
 * MQTT client interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <config/mqtt.h>
#include <string>

namespace sensateiot::mqtt
{
	class IMqttClient {
	public:
		virtual ~IMqttClient() = default;
		virtual void Connect(const config::Mqtt&) = 0;
		virtual void Publish(const std::string& topic, const std::string& msg) = 0;
	};
}
