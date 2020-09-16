/*
 * Config parser header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>

#include <config/config.h>

namespace sensateiot::parser
{
	[[nodiscard]]
	config::Config Parse(const std::string& path);
}
