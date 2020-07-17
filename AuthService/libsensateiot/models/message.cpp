/*
 * Message model implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <sensateiot/models/objectid.h>
#include <sensateiot/models/message.h>

#include <string>

namespace sensateiot::models
{
	void Message::SetSecret(const std::string& secret)
	{
		this->m_secret = secret;
	}

	const std::string& Message::GetSecret() const
	{
		return this->m_secret;
	}

	void Message::SetData(const std::string& data)
	{
		this->m_data = data;
	}

	const std::string& Message::GetData() const
	{
		return this->m_data;
	}

	void Message::SetObjectId(const ObjectId& id)
	{
		this->m_sensorId = id;
	}

	const ObjectId& Message::GetObjectId() const
	{
		return this->m_sensorId;
	}
}
