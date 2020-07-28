/*
 * Flush API key by key value command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/commands/AbstractCommandHandler.h>
#include <sensateiot/services/messageservice.h>

namespace sensateiot::commands
{
	class FlushKeyCommandHandler : public AbstractCommandHandler {
	public:
		explicit FlushKeyCommandHandler(services::MessageService& services);
		~FlushKeyCommandHandler() override = default;

		void Execute(const Command& cmd) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_messageService;
	};
}
