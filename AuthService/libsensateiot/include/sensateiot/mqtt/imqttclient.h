/*
 * MQTT client interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <config/mqtt.h>

namespace sensateiot::mqtt
{
	class IMqttClient {
	public:
		virtual void Connect(const config::Mqtt&) = 0;
	};
}
