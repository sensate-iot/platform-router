/*
 * User model implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/user.h>

#include <boost/uuid/uuid_io.hpp>
#include <boost/lexical_cast.hpp>

namespace sensateiot::models
{
	void User::SetId(const User::IdType &id)
	{
		this->m_id = id;
	}

	const User::IdType& User::GetId() const
	{
		return this->m_id;
	}

	void User::SetLockout(bool lockout)
	{
		this->m_lockout = lockout;
	}

	void User::SetId(const std::string &id)
	{
		this->m_id = boost::lexical_cast<IdType>(id);
	}

	bool User::GetLockout() const
	{
		return this->m_lockout;
	}

	bool User::GetBanned() const
	{
		return this->m_banned;
	}

	void User::SetBanned(bool banned)
	{
		this->m_banned = banned;
	}
}

