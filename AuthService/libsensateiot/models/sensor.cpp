/*
 * Sensor model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/sensor.h>

namespace sensateiot::models
{
	void Sensor::SetId(std::string id)
	{
		this->m_id = std::move(id);
	}

	void Sensor::SetSecret(std::string secret)
	{
		this->m_secret = std::move(secret);
	}

	const std::string &Sensor::GetId() const
	{
		return this->m_id;
	}

	const std::string &Sensor::GetSecret() const
	{
		return this->m_secret;
	}

	void Sensor::SetOwner(std::string owner)
	{
		this->m_owner = owner;
	}

	void Sensor::SetName(std::string name)
	{
		this->m_name = std::move(name);
	}

	const std::string &Sensor::GetOwner() const
	{
		return this->m_owner;
	}

	const std::string& Sensor::GetName() const
	{
		return this->m_name;
	}
}
