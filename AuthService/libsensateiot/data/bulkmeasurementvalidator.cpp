/*
 * JSON measurement validator implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/bulkmeasurementvalidator.h>

#include <rapidjson/document.h>
#include <rapidjson/stringbuffer.h>
#include <rapidjson/rapidjson.h>
#include <rapidjson/writer.h>

#include "parser.h"

namespace sensateiot::data
{
	std::optional<std::vector<std::pair<std::string, models::RawMeasurement>>> BulkMeasurementValidator::operator()(const std::string& str) const
	{
		std::vector<std::pair<std::string, models::RawMeasurement>> measurements;

		try {
			rapidjson::Document doc;
			doc.Parse(str.c_str());

			for(auto iter = doc.Begin(); iter != doc.End(); ++iter) {
				auto measurement = detail::ParseSingleMeasurement(iter->GetObject());

				if (!measurement.first) {
					continue;
				}


				rapidjson::StringBuffer buf;
				rapidjson::Writer<rapidjson::StringBuffer> writer(buf);
				buf.Clear();
				iter->Accept(writer);

				auto p = std::make_pair(buf.GetString(), std::move(measurement.second));
				measurements.emplace_back(std::move(p));
			}

			return std::optional(std::move(measurements));
		} catch (std::exception&) {
			return std::optional<std::vector<std::pair<std::string, models::RawMeasurement>>>();
		}
	}
}