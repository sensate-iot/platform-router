
/*
 * JSON message validator header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <vector>
#include <utility>

#include <sensateiot/models/message.h>
#include <sensateiot/data/messagevalidator.h>

namespace sensateiot::data
{
	std::pair<bool, models::Message> MessageValidator::operator()(const std::string& str) const
	{
		try {
		} catch(std::exception&) {
			
		}
		return {};
	}
}
