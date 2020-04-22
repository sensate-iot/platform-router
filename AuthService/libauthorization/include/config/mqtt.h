/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <config/mqttbroker.h>

namespace sensateiot::auth::config
{
	class DLL_EXPORT Mqtt {
	public:
		PrivateBroker& GetPrivateBroker();
		PublicBroker& GetPublicBroker();

	private:
		PrivateBroker m_private;
		PublicBroker m_public;
	};
}
