/*
 * JSON measurement validator header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <vector>
#include <utility>

#include <sensateiot/models/measurement.h>

namespace sensateiot::data
{
	struct MeasurementValidator {
		std::pair<bool, models::Measurement> operator()(const std::string& str) const;
	};
}
