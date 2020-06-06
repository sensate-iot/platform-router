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

	const std::string &Sensor::GetOwner() const
	{
		return this->m_owner;
	}


	size_t Sensor::size() const
	{
		return sizeof(*this) +
			this->m_owner.length() +
			this->m_secret.length() +
			this->m_id.length();
	}

	bool Sensor::operator==(const Sensor &other) const
	{
		return this->m_id == other.m_id;
	}

	bool Sensor::operator!=(const Sensor &other) const
	{
		return this->m_id != other.m_id;
	}
}
