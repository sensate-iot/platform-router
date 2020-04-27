/*
 * API key model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/apikey.h>

namespace sensateiot::models
{
	void ApiKey::SetKey(const std::string& str)
	{
		this->m_key = str;
	}

	void ApiKey::SetRevoked(bool revoked)
	{
		this->m_revoked = revoked;
	}

	auto ApiKey::GetKey() const -> const std::string&
	{
		return this->m_key;
	}

	auto ApiKey::GetRevoked() const -> bool
	{
		return this->m_revoked;
	}

	void ApiKey::SetUserId(const std::string &value)
	{
		this->m_userId = value;
	}

	auto ApiKey::GetUserId() const -> std::string
	{
		return this->m_userId;
	}
}
