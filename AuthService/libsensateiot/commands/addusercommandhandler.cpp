/*
 * Add user by UUID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/commands/addusercommandhandler.h>
#include <sensateiot/util/log.h>

namespace sensateiot::commands
{
	AddUserCommandHandler::AddUserCommandHandler(services::MessageService& services) : m_messageService(services)
	{
	}

	void AddUserCommandHandler::Execute(const Command& cmd)
	{
		try {
			util::Log::GetLog() << "Adding/updating user with ID: " << cmd.args << "!" << util::Log::NewLine;
			this->m_messageService->AddUser(cmd.args);
		} catch (std::exception& ex) {
			util::Log::GetLog() << "Unable to add or update user with ID: " << cmd.args << " because: " << ex.what() << util::Log::NewLine;
		}
	}
}
