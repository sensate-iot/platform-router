/*
 * Command message consumer.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/services/messageservice.h>
#include <sensateiot/consumers/commandconsumer.h>

namespace sensateiot::consumers
{
	CommandConsumer::CommandConsumer()
	{
	}

	void CommandConsumer::AddCommand(const commands::Command& cmd)
	{
		std::unique_lock l(this->m_mtx);
		this->m_commands.push_back(cmd);
	}

	void CommandConsumer::AddCommand(commands::Command&& cmd)
	{
		std::unique_lock l(this->m_mtx);
		this->m_commands.emplace_back(std::forward<commands::Command>(cmd));
	}

	void CommandConsumer::AddHandler(const std::string& key, const CommandHandler& handler)
	{
		std::unique_lock l(this->m_mtx);
		this->m_handlers.insert_or_assign(key, handler);
	}

	void CommandConsumer::AddHandler(const std::string& key, commands::AbstractCommandHandler& cmd)
	{
		std::unique_lock l(this->m_mtx);
		this->m_handlers.insert_or_assign(key, [&cmd](const commands::Command& data) {
			cmd.Execute(data);
		});
	}

	void CommandConsumer::Execute()
	{
		std::vector<commands::Command> cmds;

		{
			std::unique_lock l(this->m_mtx);

			if (this->m_commands.empty()) {
				return;
			}

			cmds.reserve(10);
			std::swap(this->m_commands, cmds);
		}

		std::shared_lock l(this->m_mtx);

		for (const auto& cmd : cmds) {
			try {
				const auto& handler = this->m_handlers.at(cmd.command);
				handler(cmd);
			} catch (std::out_of_range&) {
				util::Log::GetLog() << "Invalid command received: " << cmd.command << "!" << util::Log::NewLine;
			}
		}
	}
}
