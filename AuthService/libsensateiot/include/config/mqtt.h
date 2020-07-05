/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <config/mqttbroker.h>

namespace sensateiot::config
{
	class DLL_EXPORT Mqtt {
	public:
		PrivateBroker& GetPrivateBroker();
		PublicBroker& GetPublicBroker();
		
		const PrivateBroker& GetPrivateBroker() const;
		const PublicBroker& GetPublicBroker() const;

	private:
		PrivateBroker m_private{};
		PublicBroker m_public{};
	};
}
