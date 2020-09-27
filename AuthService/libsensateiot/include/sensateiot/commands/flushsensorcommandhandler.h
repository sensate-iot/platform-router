/*
 * Flush sensor by object ID command handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/commands/abstractcommandhandler.h>
#include <sensateiot/services/messageservice.h>

namespace sensateiot::commands
{
	class FlushSensorCommandHandler : public AbstractCommandHandler {
	public:
		explicit FlushSensorCommandHandler(services::MessageService& services);
		~FlushSensorCommandHandler() override = default;

		void Execute(const Command& cmd) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_messageService;
	};
}
