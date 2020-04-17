/*
 * API key model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>

namespace sensateiot::auth
{
	namespace detail
	{
		enum ApiKeyType {
			SensorKey,
			SystemKey,
			ApiKey
		};
	}

	class ApiKey {
	public:
		typedef detail::ApiKeyType Type;

		void SetKey(const std::string& value);
		void SetType(Type type);
		void SetRevoked(bool revoked);

		[[nodiscard]]
		auto GetKey() const -> const std::string&;

		[[nodiscard]]
		auto GetType() const -> Type;

		[[nodiscard]]
		auto GetRevoked() const -> bool;

	private:
		std::string m_key;
		Type m_type;
		bool m_revoked;
	};
}
