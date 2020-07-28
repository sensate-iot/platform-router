/*
 * Plain old Data for commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <string>
#include <optional>
#include <string_view>

namespace sensateiot::commands
{
	struct Command {
		std::string command;
		std::string args;

		static std::optional<Command> FromJson(const std::string& str);

	private:
		static constexpr auto CmdKey = std::string_view("cmd");
		static constexpr auto ArgKey = std::string_view("args");
	};
}
