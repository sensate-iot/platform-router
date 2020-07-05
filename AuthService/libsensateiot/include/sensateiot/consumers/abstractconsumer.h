/*
 * Abstract measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <utility>
#include <string>
#include <vector>

#include <sensateiot/models/objectid.h>

namespace sensateiot::consumers
{
	template <typename T>
	class AbstractConsumer {
	public:
		typedef T ModelType;
		typedef std::pair<std::string, ModelType> MessagePair;
		typedef std::pair<std::size_t, std::vector<models::ObjectId>> ProcessingStats;

		virtual ~AbstractConsumer() = default;
		
		virtual void PushMessage(MessagePair data) = 0;
		virtual std::size_t PostProcess() = 0;
		virtual ProcessingStats Process() = 0;

	protected:
		static constexpr int SecretSubStringOffset = 3;
		static constexpr int SecretSubstringStart = 1;
		static constexpr auto SearchRegex = std::string_view("\\$[a-f0-9]{64}==");
	};
}
