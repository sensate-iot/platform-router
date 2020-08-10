
/*
 * JSON message validator header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <vector>
#include <utility>

#include <sensateiot/models/message.h>
#include <sensateiot/data/messagevalidator.h>

#include <rapidjson/document.h>
#include <rapidjson/rapidjson.h>

#include "parser.h"

namespace sensateiot::data
{
	std::pair<bool, models::Message> MessageValidator::operator()(const std::string& str) const
	{
		try {
			rapidjson::Document json;
			json.Parse(str.c_str());

			if(!json.IsObject()) {
				return {};
			}

			return detail::ParseSingleMessage(json);
		} catch(std::exception &) {
			return {};
		}
	}
}
