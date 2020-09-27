/*
 * Add user by UUID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/commands/addkeycommandhandler.h>
#include <sensateiot/util/log.h>

namespace sensateiot::commands
{
	AddKeyCommandHandler::AddKeyCommandHandler(services::MessageService& services) : m_messageService(services)
	{
	}

	void AddKeyCommandHandler::Execute(const Command& cmd)
	{
		try {
			util::Log::GetLog() << "Adding/updating sensor key!" << util::Log::NewLine;
			this->m_messageService->AddKey(cmd.args);
		} catch (std::exception& ex) {
			util::Log::GetLog() << "Unable to add/update key because: " << ex.what() << util::Log::NewLine;
		}
	}
}
