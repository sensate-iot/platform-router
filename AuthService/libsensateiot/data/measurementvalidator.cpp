/*
 * JSON measurement validator implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/measurementvalidator.h>

#include <rapidjson/document.h>
#include <rapidjson/rapidjson.h>

#include "parser.h"

namespace sensateiot::data
{
	std::pair<bool, models::RawMeasurement> MeasurementValidator::operator()(const std::string &str) const
	{
		try {
			rapidjson::Document json;
			json.Parse(str.c_str());

			return detail::ParseSingleMeasurement(json);
		} catch(std::exception &) {
			return std::make_pair(false, models::RawMeasurement());
		}
	}
}
