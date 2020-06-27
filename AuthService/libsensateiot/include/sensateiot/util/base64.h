/*
 * Base64 header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <vector>
#include <sensateiot.h>

namespace sensateiot::util
{
	extern DLL_EXPORT std::string Encode64(const std::vector<std::uint8_t>& input);
}
