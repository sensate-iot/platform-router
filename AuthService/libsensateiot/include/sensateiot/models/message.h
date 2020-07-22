/*
 * Message model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <sensateiot/models/objectid.h>

#include <string>

namespace sensateiot::models
{
	class Message {
	public:
		void SetSecret(const std::string& secret);
		[[nodiscard]] const std::string& GetSecret() const;
		
		void SetData(const std::string& data);
		[[nodiscard]] const std::string& GetData() const;

		void SetCreatedAt(const std::string& date);
		[[nodiscard]] const std::string& GetCreatedAt() const;
		
		void SetObjectId(const ObjectId& id);
		[[nodiscard]] const ObjectId& GetObjectId() const;

	private:
		std::string m_secret;
		std::string m_data;
		std::string m_createdAt;
		ObjectId m_sensorId;
	};
}
