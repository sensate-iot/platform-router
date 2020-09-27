/*
 * Abstract measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/commands/command.h>

namespace sensateiot::commands
{
	class AbstractCommandHandler {
	public:
		virtual ~AbstractCommandHandler() = default;
		virtual void Execute(const Command& cmd) = 0;
	};
}
