/*
 * Sensor model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>

#include <string>

namespace sensateiot::auth
{
	class DLL_EXPORT Sensor {
	public:
		void SetId(std::string id);
		void SetSecret(std::string secret);

		[[nodiscard]]
		const std::string& GetSecret() const;

		[[nodiscard]]
		const std::string& GetId() const;

	private:
		std::string m_id;
		std::string m_secret;
	};
}
