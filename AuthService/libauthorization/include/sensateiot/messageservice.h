/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <config/config.h>

#include <sensateiot/measurementhandler.h>

namespace sensateiot::mqtt
{
	class MessageService {
	public:
		explicit MessageService(InternalMqttClient& client, const config::Config& conf);

	private:
		config::Config m_conf;
		std::vector<MeasurementHandler> m_handlers;
	};
}
