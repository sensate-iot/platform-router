/*
 * Flush user by UUID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/commands/addsensorcommandhandler.h>
#include <sensateiot/util/log.h>

namespace sensateiot::commands
{
	AddSensorCommandHandler::AddSensorCommandHandler(services::MessageService& services) : m_messageService(services)
	{
	}

	void AddSensorCommandHandler::Execute(const Command& cmd)
	{
		try {
			util::Log::GetLog() << "Adding/updating sensor with ID: " << cmd.args << "!" << util::Log::NewLine;
			this->m_messageService->AddSensor(cmd.args);
		} catch (std::exception& ex) {
			util::Log::GetLog() << "Unable to add or update sensor with ID: " << cmd.args << " because: " << ex.what() << util::Log::NewLine;
		}
	}
}
