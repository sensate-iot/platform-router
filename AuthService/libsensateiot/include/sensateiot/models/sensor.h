/*
 * Sensor model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <sensateiot/models/objectid.h>

#include <boost/uuid/uuid.hpp>
#include <string>

namespace sensateiot::models
{
	class DLL_EXPORT Sensor {
	public:
		void SetId(const ObjectId& id);
		void SetId(const std::string& id);
		void SetSecret(std::string secret);
		void SetOwner( const std::string& owner);
		void SetOwner(boost::uuids::uuid owner);

		[[nodiscard]]
		const std::string& GetSecret() const;

		[[nodiscard]]
		const ObjectId& GetId() const;

		[[nodiscard]]
		const boost::uuids::uuid& GetOwner() const;

		[[nodiscard]]
		size_t size() const;

		bool operator==(const Sensor& other) const;
		bool operator!=(const Sensor& other) const;

	private:
		ObjectId m_id;
		std::string m_secret;
		boost::uuids::uuid m_owner{};
	};
}
