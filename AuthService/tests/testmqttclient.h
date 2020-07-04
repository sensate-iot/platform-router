/*
 * Mocked MQTT client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/mqtt/imqttclient.h>

#include <string>
#include <iostream>

namespace sensateiot::test
{
	class TestMqttClient : public mqtt::IMqttClient {

	public:
		void Connect(const config::Mqtt &mqtt) override
		{
		}
		
		void Publish(const std::string& topic, const std::string& msg) override
		{
			std::cout << "Publish on topic: " << topic << " || Message: " << msg << std::endl;
		}
	};
}
