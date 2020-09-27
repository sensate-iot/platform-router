/*
 * GZIP compression/decompression.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <vector>

#include <sensateiot.h>

namespace sensateiot::util
{
	extern DLL_EXPORT std::string Compress(const std::vector<char>& data);
}
