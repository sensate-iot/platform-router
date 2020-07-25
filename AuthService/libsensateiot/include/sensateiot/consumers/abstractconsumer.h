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
#include <shared_mutex>

#include <re2/re2.h>

#include <sensateiot/models/objectid.h>
#include <sensateiot/util/protobuf.h>
#include <sensateiot/util/gzip.h>

namespace sensateiot::consumers
{
	template <typename T>
	class AbstractConsumer {
	public:
		typedef T ModelType;
		typedef std::pair<std::string, ModelType> MessagePair;
		typedef std::pair<std::size_t, std::vector<models::ObjectId>> ProcessingStats;

		explicit AbstractConsumer(mqtt::IMqttClient& client, data::DataCache& cache, config::Config conf) :
			m_internal(client), m_cache(cache), m_config(std::move(conf)), m_regex(SearchRegex.data())
		{
			std::scoped_lock l(this->m_lock);
			this->m_messages.reserve(MessageArraySize);
		}

		AbstractConsumer(AbstractConsumer&& rhs) noexcept : m_internal(std::move(rhs.m_internal)),
			m_cache(std::move(rhs.m_cache)), m_config(std::move(rhs.m_config)),
			m_regex(SearchRegex.data()), m_messages(std::move(rhs.m_messages))
		{
		}
		
		virtual ~AbstractConsumer() = default;
		
		virtual void PushMessage(MessagePair data)
		{
			std::scoped_lock l(this->m_lock);
			this->m_messages.emplace_back(std::move(data));
		}
		
		virtual void PushMessages(std::vector<MessagePair>&& data)
		{
			std::scoped_lock lock(this->m_lock);

			std::move(data.begin(), data.end(), std::back_inserter(this->m_messages));
			data.clear();
		}
		
		virtual std::size_t PostProcess() = 0;
		virtual ProcessingStats Process() = 0;

		virtual void Move(AbstractConsumer& other)
		{
			this->m_config = std::move(other.m_config);
			this->m_cache = std::move(other.m_cache);
			this->m_internal = std::move(other.m_internal);
			this->m_messages = std::move(other.m_messages);
		}
		
		virtual void Copy(AbstractConsumer& other)
		{
			this->m_config = other.m_config;
			this->m_internal = other.m_internal;
			this->m_cache = other.m_cache;
			this->m_messages = other.m_messages;
		}

	protected:
		static constexpr int SecretSubStringOffset = 3;
		static constexpr int SecretSubstringStart = 1;
		static constexpr auto SearchRegex = std::string_view("\\$[a-f0-9]{64}==");
		static constexpr auto MessageArraySize = 100000;

		stl::ReferenceWrapper<mqtt::IMqttClient> m_internal;
		stl::ReferenceWrapper<data::DataCache> m_cache;
		config::Config m_config;
		RE2 m_regex;
		std::vector<MessagePair> m_messages;
		std::shared_mutex m_lock;

		/* Methods */
		virtual void PublishAuthorizedMessages(const std::vector<ModelType>& authorized, const std::string& topic) 
		{
			std::vector<ns_base::mqtt::delivery_token_ptr> tokens;

			try {
				for(std::size_t idx = 0UL; idx < authorized.size(); idx += this->m_config.GetInternalBatchSize()) {
					auto begin = authorized.begin() + idx;
					auto endIdx = (idx + this->m_config.GetInternalBatchSize() <= authorized.size()) ?
						idx + this->m_config.GetInternalBatchSize() : authorized.size();
					auto end = authorized.begin() + endIdx;

					auto data = util::Compress(util::to_protobuf(begin, end));
					auto token = this->m_internal->Publish(topic, data);
					tokens.push_back(token);
				}

				for(auto&& token : tokens) {
					if (!token) {
						continue;
					}

					if(token->is_complete()) {
						continue;
					}

					token->wait();
				}
			} catch(ns_base::mqtt::exception& ex) {
				auto& log = util::Log::GetLog();
				log << "Unable to publish messages: " << ex.get_error_str() << util::Log::NewLine;
			}
		}
	};
}
