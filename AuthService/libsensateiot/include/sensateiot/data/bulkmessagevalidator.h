/*
 * Bulk message validator.
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
	struct BulkMessageValidator {
		std::optional<std::vector<std::pair<std::string, models::Message>>> operator()(const std::string& str) const;
	};
}
