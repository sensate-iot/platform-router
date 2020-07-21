/*
 * JSON serialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/models/measurement.h>

#include <string>

namespace sensateiot::util
{
	template <typename T>
	std::vector<char> to_protobuf(const T& value)
	{
		throw new std::logic_error("Method not implemented!");
	}
	

	template <>
	std::vector<char> to_protobuf<std::vector<models::Measurement>>(const std::vector<models::Measurement>& value);

	std::vector<char> to_protobuf(
		std::vector<models::Measurement>::const_iterator begin,
		std::vector<models::Measurement>::const_iterator end
	);
}
