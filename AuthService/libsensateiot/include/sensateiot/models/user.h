/*
 * User model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <string>

#include <boost/uuid/uuid.hpp>

namespace sensateiot::models
{
	class DLL_EXPORT User {
	public:
		typedef boost::uuids::uuid IdType;

		explicit User() = default;
		User(const User& user) = default;
		User(User&& user) noexcept = default;

		User& operator=(const User& other) = default;
		User& operator=(User&& other) noexcept = default;

		void SetId(const IdType& id);
		void SetId(const std::string& id);

		void SetLockout(bool lockout);
		void SetBanned(bool banned);

		[[nodiscard]]
		const IdType& GetId() const;

		[[nodiscard]]
		bool GetLockout() const;

		[[nodiscard]]
		bool GetBanned() const;

	private:
		IdType m_id{};
		bool m_lockout{};
		bool m_banned{};
	};
}
