/*
 * Sensor model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <string>

namespace sensateiot::models
{
	class DLL_EXPORT Sensor {
	public:
		void SetId(std::string id);
		void SetSecret(std::string secret);
		void SetOwner(std::string owner);

		[[nodiscard]]
		const std::string& GetSecret() const;

		[[nodiscard]]
		const std::string& GetId() const;

		[[nodiscard]]
		const std::string& GetOwner() const;

		[[nodiscard]]
		size_t size() const;

	private:
		std::string m_id;
		std::string m_secret;
		std::string m_owner;
	};
}
