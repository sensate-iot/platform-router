/*
 * GZIP compression/decompression.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <sensateiot.h>

namespace sensateiot::util
{
	extern DLL_EXPORT std::string Compress(const std::string& data);
	extern DLL_EXPORT std::string Decompress(const std::string& data);
}
