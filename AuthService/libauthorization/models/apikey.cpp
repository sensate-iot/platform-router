/*
 * API key model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/apikey.h>

namespace sensateiot::auth
{
	void ApiKey::SetKey(const std::string& str)
	{
		this->m_key = str;
	}

	void ApiKey::SetType(Type type)
	{
		this->m_type = type;
	}

	void ApiKey::SetRevoked(bool revoked)
	{
		this->m_revoked = revoked;
	}

	auto ApiKey::GetKey() const -> const std::string&
	{
		return this->m_key;
	}

	auto ApiKey::GetType() const -> ApiKey::Type
	{
		return this->m_type;
	}

	auto ApiKey::GetRevoked() const -> bool
	{
		return this->m_revoked;
	}
}
