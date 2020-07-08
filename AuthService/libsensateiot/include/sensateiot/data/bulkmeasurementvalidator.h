/*
 * Bulk measurement validator.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <vector>
#include <utility>

#include <sensateiot/models/rawmeasurement.h>

namespace sensateiot::data
{
	struct BulkMeasurementValidator {
		std::optional<std::vector<std::pair<std::string, models::RawMeasurement>>> operator()(const std::string& str) const;
	};
}
