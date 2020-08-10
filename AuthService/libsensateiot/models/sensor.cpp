/*
 * Sensor model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/sensor.h>

#include <boost/uuid/uuid_io.hpp>
#include <boost/lexical_cast.hpp>

namespace sensateiot::models
{
	void Sensor::SetId(const ObjectId& id)
	{
		this->m_id = id;
	}

	void Sensor::SetId(const std::string &id)
	{
		this->m_id = ObjectId(id);
	}

	void Sensor::SetSecret(std::string secret)
	{
		this->m_secret = std::move(secret);
	}

	const ObjectId& Sensor::GetId() const
	{
		return this->m_id;
	}

	const std::string &Sensor::GetSecret() const
	{
		return this->m_secret;
	}

	void Sensor::SetOwner( const std::string& owner)
	{
		this->m_owner = boost::lexical_cast<boost::uuids::uuid>(owner);
	}

	const boost::uuids::uuid &Sensor::GetOwner() const
	{
		return this->m_owner;
	}


	size_t Sensor::size() const
	{
		return sizeof(*this) +
			this->m_owner.size() +
			this->m_secret.length();
	}

	bool Sensor::operator==(const Sensor &other) const
	{
		return this->m_id.Value() == other.m_id.Value();
	}

	bool Sensor::operator!=(const Sensor &other) const
	{
		return this->m_id.Value() != other.m_id.Value();
	}

	void Sensor::SetOwner(boost::uuids::uuid owner)
	{
		this->m_owner = owner;
	}
}
