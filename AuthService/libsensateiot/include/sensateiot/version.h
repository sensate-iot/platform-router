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
	constexpr auto VersionMajor = 0;
	constexpr auto VersionMinor = 5;
	constexpr auto PatchLevel   = 1;

	static const std::string VersionString = std::string("v") +
		std::to_string(VersionMajor) + "." +
		std::to_string(VersionMinor) + "." +
		std::to_string(PatchLevel);
}
