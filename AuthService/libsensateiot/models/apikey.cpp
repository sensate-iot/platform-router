/*
 * API key model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/apikey.h>

#include <boost/uuid/uuid_io.hpp>
#include <boost/lexical_cast.hpp>

namespace sensateiot::models
{
	void ApiKey::SetKey(const std::string& str)
	{
		this->m_key = str;
	}

	void ApiKey::SetUserId(const IdType& value)
	{
		this->m_userId = value;
	}

	void ApiKey::SetRevoked(bool revoked)
	{
		this->m_revoked = revoked;
	}

	void ApiKey::SetReadOnly(bool value)
	{
		this->m_readOnly = value;
	}

	auto ApiKey::GetKey() const -> const std::string&
	{
		return this->m_key;
	}

	auto ApiKey::GetRevoked() const -> bool
	{
		return this->m_revoked;
	}

	auto ApiKey::GetReadOnly() const -> bool
	{
		return this->m_readOnly;
	}

	void ApiKey::SetUserId(const std::string &value)
	{
		this->m_userId = boost::lexical_cast<IdType>(value);
	}

	auto ApiKey::GetUserId() const -> boost::uuids::uuid
	{
		return this->m_userId;
	}

	auto ApiKey::GetType() const -> Type
	{
		return this->m_type;
	}

	void ApiKey::SetType(Type type)
	{
		this->m_type = type;
	}
}
