/*
 * Add API key by key value command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/services/messageservice.h>

namespace sensateiot::commands
{
	class AddKeyCommandHandler : public AbstractCommandHandler {
	public:
		explicit AddKeyCommandHandler(services::MessageService& services);
		~AddKeyCommandHandler() override = default;

		void Execute(const Command& cmd) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_messageService;
	};
}
