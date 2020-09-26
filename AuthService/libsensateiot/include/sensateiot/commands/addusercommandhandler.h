/*
 * Add user by UUID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/services/messageservice.h>

namespace sensateiot::commands
{
	class AddUserCommandHandler : public AbstractCommandHandler {
	public:
		explicit AddUserCommandHandler(services::MessageService& services);
		~AddUserCommandHandler() override = default;

		void Execute(const Command& cmd) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_messageService;
	};
}
