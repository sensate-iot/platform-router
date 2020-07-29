/*
 * Plain old Data for commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/commands/command.h>
#include <sensateiot/util/log.h>

#include <rapidjson/rapidjson.h>
#include <rapidjson/document.h>

namespace sensateiot::commands
{
	std::optional<Command> Command::FromJson(const std::string& str)
	{
		Command cmd;

		try {
			rapidjson::Document json;

			json.Parse(str.c_str());

			if (!json.IsObject()) {
				return {};
			}

			if (!json.HasMember(CmdKey.data()) || !json[CmdKey.data()].IsString()) {
				return {};
			}

			if (!json.HasMember(ArgKey.data()) || !json[ArgKey.data()].IsString()) {
				return {};
			}

			cmd.command = json[CmdKey.data()].GetString();
			cmd.args = json[ArgKey.data()].GetString();
		} catch (std::exception& ex) {
			util::Log::GetLog() << "Unable to parse command: " << ex.what() << util::Log::NewLine;
			return {};
		}

		return std::make_optional(std::move(cmd));
	}
}
