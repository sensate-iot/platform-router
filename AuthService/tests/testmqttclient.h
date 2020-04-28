/*
 * Mocked MQTT client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/mqtt/imqttclient.h>

namespace sensateiot::test
{
	class TestMqttClient : public mqtt::IMqttClient {

	public:
		void Connect(const config::Mqtt &mqtt) override
		{
		}
	};
}
