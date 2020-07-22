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

namespace sensateiot::data
{
	struct MessageValidator {
		std::pair<bool, models::Message> operator()(const std::string& str) const;
	};
}
