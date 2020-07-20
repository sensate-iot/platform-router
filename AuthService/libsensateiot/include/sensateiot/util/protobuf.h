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
	std::vector<char> to_protobuf(const T& value)
	{
		throw new std::logic_error("Method not implemented!");
	}
	

	template <>
	std::vector<char> to_protobuf<std::vector<models::RawMeasurement>>(const std::vector<models::RawMeasurement>& value);

	std::vector<char> to_protobuf(
		std::vector<models::RawMeasurement>::const_iterator begin,
		std::vector<models::RawMeasurement>::const_iterator end
	);
}
