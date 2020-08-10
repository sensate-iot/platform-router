/*
 * API key model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <string>

namespace sensateiot::models
{
	namespace detail
	{
		enum class ApiKeyType {
			SensorKey,
			SystemKey,
			ApiKey
		};
	}

	class DLL_EXPORT ApiKey {
	public:
		explicit ApiKey() = default;
		typedef detail::ApiKeyType Type;

		void SetKey(const std::string& value);
		void SetRevoked(bool revoked);
		void SetType(Type type);
		void SetUserId(const std::string& value);

		[[nodiscard]]
		auto GetKey() const -> const std::string&;

		[[nodiscard]]
		auto GetRevoked() const -> bool;

		[[nodiscard]]
		auto GetUserId() const -> std::string;

		[[nodiscard]]
		auto GetType() const -> Type;

	private:
		std::string m_key;
		std::string m_userId;
		Type m_type { detail::ApiKeyType::SystemKey };
		bool m_revoked {};
	};
}
