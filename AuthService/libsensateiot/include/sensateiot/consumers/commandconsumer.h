/*
 * Command message consumer.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/stl/dictionary.h>
#include <sensateiot/services/messageservice.h>
#include <sensateiot/commands/AbstractCommandHandler.h>
#include <sensateiot/commands/command.h>

#include <functional>
#include <limits>
#include <vector>
#include <shared_mutex>

#include <boost/unordered_map.hpp>

namespace sensateiot::consumers
{
	class CommandConsumer {
	public:
		typedef std::function<void(const commands::Command&)> CommandHandler;
		explicit CommandConsumer();

		void AddCommand(const commands::Command& cmd);
		void AddCommand(commands::Command&& cmd);
		void AddHandler(const std::string& key, const CommandHandler& handler);
		void AddHandler(const std::string& key, commands::AbstractCommandHandler& cmd);
		void Execute();

	private:
		static constexpr auto HandlerTimeout = std::numeric_limits<long>::max();

		std::shared_mutex m_mtx;
		boost::unordered_map<std::string, CommandHandler> m_handlers;
		std::vector<commands::Command> m_commands;
	};
}
