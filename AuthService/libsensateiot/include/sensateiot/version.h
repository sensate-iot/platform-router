/*
 * Version constant header for libsensateiot.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>

namespace sensateiot::version
{
	constexpr auto VersionMajor = 1;
	constexpr auto VersionMinor = 0;
	constexpr auto PatchLevel   = 0;

	static const std::string VersionString =
		std::to_string(VersionMajor) + "." +
		std::to_string(VersionMinor) + "." +
		std::to_string(PatchLevel);
}
