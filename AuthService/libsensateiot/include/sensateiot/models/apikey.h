/*
 * API key model definition.
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
	enum class ApiKeyType {
		SensorKey,
		SystemKey,
		ApiKey
	};

	class DLL_EXPORT ApiKey {
	public:
		explicit ApiKey() = default;
		
		typedef ApiKeyType Type;
		typedef boost::uuids::uuid IdType;

		void SetKey(const std::string& value);
		void SetRevoked(bool revoked);
		void SetReadOnly(bool value);
		void SetType(Type type);
		void SetUserId(const std::string& value);
		void SetUserId(const IdType& value);

		[[nodiscard]]
		auto GetKey() const -> const std::string&;

		[[nodiscard]]
		auto GetRevoked() const -> bool;
		
		[[nodiscard]]
		auto GetReadOnly() const -> bool;

		[[nodiscard]]
		auto GetUserId() const -> IdType;

		[[nodiscard]]
		auto GetType() const -> Type;

	private:
		std::string m_key;
		IdType m_userId{};
		Type m_type { ApiKeyType::SystemKey };
		bool m_revoked {};
		bool m_readOnly{};
	};
}
