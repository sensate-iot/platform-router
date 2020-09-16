/*
 * Flush user by UUID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/commands/flushkeycommandhandler.h>
#include <sensateiot/util/log.h>

namespace sensateiot::commands
{
	FlushKeyCommandHandler::FlushKeyCommandHandler(services::MessageService& services) : m_messageService(services)
	{
	}

	void FlushKeyCommandHandler::Execute(const Command& cmd)
	{
		try {
			util::Log::GetLog() << "Flushing sensor key!" << util::Log::NewLine;
			this->m_messageService->FlushKey(cmd.args);
		} catch (std::exception& ex) {
			util::Log::GetLog() << "Unable to flush key because: " << ex.what() << util::Log::NewLine;
		}
	}
}
