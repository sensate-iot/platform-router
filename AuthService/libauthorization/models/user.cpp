/*
 * User model implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/user.h>

namespace sensateiot::auth
{
	const std::string& User::GetId() const
	{
		return this->m_id;
	}

	void User::SetLockout(bool lockout)
	{
		this->m_lockout = lockout;
	}

	void User::SetId(const std::string &id)
	{
		this->m_id = id;
	}

	bool User::GetLockout() const
	{
		return this->m_lockout;
	}
}

