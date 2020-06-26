/*
 * JSON serialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/models/rawmeasurement.h>

#include <string>

namespace sensateiot::util
{
	template <typename T>
	std::string to_protobuf(const T& value)
	{
		throw new std::logic_error("Method not implemented!");
	}

	template <>
	std::string to_protobuf<models::RawMeasurement>(const models::RawMeasurement& value);

	template <>
	std::string to_protobuf<std::vector<models::RawMeasurement>>(const std::vector<models::RawMeasurement>& value);
}
