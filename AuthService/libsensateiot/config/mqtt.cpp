/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/mqtt.h>

namespace sensateiot::config
{
	const PrivateBroker& Mqtt::GetPrivateBroker() const
	{
		return this->m_private;
	}

	PrivateBroker &Mqtt::GetPrivateBroker()
	{
		return this->m_private;
	}
}
