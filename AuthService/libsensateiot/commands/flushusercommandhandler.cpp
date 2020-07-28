/*
 * Flush user by UUID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/commands/AbstractCommandHandler.h>
#include <sensateiot/commands/flushusercommandhandler.h>
#include <sensateiot/util/log.h>

namespace sensateiot::commands
{
	FlushUserCommandHandler::FlushUserCommandHandler(services::MessageService& services) : m_messageService(services)
	{
	}

	void FlushUserCommandHandler::Execute(const Command& cmd)
	{
		try {
			util::Log::GetLog() << "Flushing user with ID: " << cmd.args << "!" << util::Log::NewLine;
			this->m_messageService->FlushUser(cmd.args);
		} catch (std::exception& ex) {
			util::Log::GetLog() << "Unable to flush user with ID: " << cmd.args << " because: " << ex.what() << util::Log::NewLine;
		}
	}
}
