/*
 * JSON messsage validator implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/bulkmessagevalidator.h>
#include <sensateiot/util/log.h>

#include <rapidjson/document.h>
#include <rapidjson/stringbuffer.h>
#include <rapidjson/rapidjson.h>
#include <rapidjson/writer.h>

#include "parser.h"

namespace sensateiot::data
{
	std::optional<std::vector<std::pair<std::string, models::Message>>> BulkMessageValidator::operator()( const std::string& str) const
	{
		std::vector<std::pair<std::string, models::Message>> messsages;

		try {
			rapidjson::Document doc;
			doc.Parse(str.c_str());

			if(!doc.IsArray()) {
				return std::optional<std::vector<std::pair<std::string, models::Message>>>();
			}

			for(auto iter = doc.Begin(); iter != doc.End(); ++iter) {
				auto messsage = detail::ParseSingleMessage(iter->GetObject());

				if (!messsage.first) {
					continue;
				}

				rapidjson::StringBuffer buf;
				rapidjson::Writer<rapidjson::StringBuffer> writer(buf);
				buf.Clear();
				iter->Accept(writer);

				auto p = std::make_pair(buf.GetString(), std::move(messsage.second));
				messsages.emplace_back(std::move(p));
			}

			return std::optional(std::move(messsages));
		} catch (std::exception& ex) {
			auto& log = util::Log::GetLog();

			log << "Unable to parse bulk messages: " << ex.what() << util::Log::NewLine;
			return {};
		}
	}
}
