/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/mqtt.h>

namespace sensateiot::config
{
	PublicBroker &Mqtt::GetPublicBroker()
	{
		return this->m_public;
	}

	PrivateBroker &Mqtt::GetPrivateBroker()
	{
		return this->m_private;
	}
}
