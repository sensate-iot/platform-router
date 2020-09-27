/*
 * Mocked MQTT client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/mqtt/imqttclient.h>
#include <sensateiot/util/log.h>

#include <string>
#include <iostream>

namespace sensateiot::test
{
	class TestMqttClient : public mqtt::IMqttClient {

	public:
		void Connect(const config::Mqtt &mqtt) override
		{
		}
		
		ns_base::mqtt::delivery_token_ptr Publish(const std::string& topic, const std::string& msg) override
		{
			auto& log = util::Log::GetLog();
			std::stringstream value;

			value << "Publish on topic: " << topic;
			log << value.str() << util::Log::NewLine;

			return {};
		}
	};
}
