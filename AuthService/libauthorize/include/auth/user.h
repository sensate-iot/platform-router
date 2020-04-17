/*
 * User model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>

namespace sensateiot::auth
{
	class User {
	public:
		void SetId(const std::string& id);
		void SetLockout(bool lockout);

		[[nodiscard]]
		const std::string& GetId() const;

		[[nodiscard]]
		bool GetLockout() const;

	private:
		std::string m_id;
		bool m_lockout;
	};
}
