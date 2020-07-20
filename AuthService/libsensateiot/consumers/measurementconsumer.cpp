/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <iostream>

#include <sensateiot/consumers/measurementconsumer.h>
#include <sensateiot/util/log.h>
#include <sensateiot/util/sha256.h>
#include <sensateiot/util/protobuf.h>
#include <sensateiot/util/gzip.h>

#include <mqtt/async_client.h>

namespace sensateiot::consumers
{
	MeasurementConsumer::MeasurementConsumer(mqtt::IMqttClient &client, data::DataCache &cache, config::Config conf) :
		m_internal(client),
		m_cache(cache),
		m_regex(SearchRegex.data()),
		m_config(std::move(conf))
	{
		this->m_measurements.reserve(100000);
	}

	MeasurementConsumer::~MeasurementConsumer()
	{
		this->m_lock.lock();
		this->m_measurements.clear();
		this->m_lock.unlock();
	}

	MeasurementConsumer::MeasurementConsumer(MeasurementConsumer &&rhs) noexcept :
		m_regex(SearchRegex.data()), m_config(std::move(rhs.m_config))
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
		this->m_cache = std::move(rhs.m_cache);
		this->m_leftOver = std::move(rhs.m_leftOver);
	}

	MeasurementConsumer &MeasurementConsumer::operator=(MeasurementConsumer &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
		this->m_cache = std::move(rhs.m_cache);
		this->m_config = std::move(rhs.m_config);
		this->m_leftOver = std::move(rhs.m_leftOver);

		return *this;
	}

	bool MeasurementConsumer::ValidateMeasurement(const models::Sensor& sensor, MessagePair & pair) const
	{
		auto result = RE2::Replace(&pair.first, this->m_regex, sensor.GetSecret());

		if(result) {
			auto offset = pair.second.GetKey().length() - SecretSubStringOffset;
			auto key = pair.second.GetKey().substr(SecretSubstringStart, offset);
			return util::sha256_compare(pair.first, key);
		}

		/* This is not a SHA256 secured message. Authorize manually. */
		return pair.second.GetKey() == sensor.GetSecret();
	}

	void MeasurementConsumer::PublishAuthorizedMessages(const std::vector<models::RawMeasurement>& authorized) 
	{
		std::vector<ns_base::mqtt::delivery_token_ptr> tokens;

		try {
			for(std::size_t idx = 0UL; idx < authorized.size(); idx += this->m_config.GetInternalBatchSize()) {
				auto begin = authorized.begin() + idx;
				auto endIdx = (idx + this->m_config.GetInternalBatchSize() <= authorized.size()) ?
					idx + this->m_config.GetInternalBatchSize() : authorized.size();
				auto end = authorized.begin() + endIdx;

				auto data = util::Compress(util::to_protobuf(begin, end));
				auto token = this->m_internal->Publish(this->m_config.GetMqtt().GetPrivateBroker().GetBulkMeasurementTopic(), data);
				tokens.push_back(token);
			}

			for(auto&& token : tokens) {
				if(token->is_complete()) {
					continue;
				}

				token->wait();
			}
		} catch(ns_base::mqtt::exception& ex) {
			auto& log = util::Log::GetLog();
			log << "Unable to publish mesages: " << ex.get_error_str() << util::Log::NewLine;
		}
	}

	void MeasurementConsumer::PushMessage(MessagePair measurement)
	{
		std::scoped_lock l(this->m_lock);
		this->m_measurements.emplace_back(std::move(measurement));
	}

	void MeasurementConsumer::PushMessages(std::vector<MessagePair>&& measurements)
	{
		std::scoped_lock lock(this->m_lock);

		std::move(measurements.begin(), measurements.end(), std::back_inserter(this->m_measurements));
		measurements.clear();
	}

	std::size_t MeasurementConsumer::PostProcess()
	{
		std::vector<MessagePair> data;
		SensorLookupType sensor;

		this->m_lock.lock();

		if(this->m_leftOver.empty()) {
			this->m_lock.unlock();
			return {};
		}
		
		std::swap(this->m_leftOver, data);
		this->m_leftOver.clear();
		this->m_lock.unlock();

		std::vector<models::RawMeasurement> authorized;
		authorized.reserve(data.size());

		std::sort(std::begin(data), std::end(data), [](const auto& x, const auto& y)
		{
			return x.second.GetObjectId().compare(y.second.GetObjectId()) < 0;
		});

		for(auto&& pair : data) {
			if(!sensor.second.has_value() || sensor.second->GetId() != pair.second.GetObjectId()) {
				sensor = this->m_cache->GetSensor(pair.second.GetObjectId());
			}

			if(!sensor.first || !sensor.second.has_value()) {
				continue;
			}

			/* Valid sensor. Validate the measurement. */
			if(!this->ValidateMeasurement(sensor.second.value(), pair)) {
				continue;
			}

			authorized.emplace_back(std::move(pair.second));
		}

		/* Package authorized */
		if(authorized.empty()) {
			return {};
		}

		this->PublishAuthorizedMessages(authorized);
		return authorized.size();
	}

	MeasurementConsumer::ProcessingStats MeasurementConsumer::Process()
	{
		std::vector<MessagePair> data;
		std::vector<MessagePair> leftOver;
		std::vector<models::ObjectId> notFound;
		SensorLookupType sensor;
		auto& log = util::Log::GetLog();

		this->m_lock.lock();
		data.reserve(std::max<std::size_t>(this->m_measurements.size(), 100000UL));
		std::swap(this->m_measurements, data);
		this->m_measurements.clear();
		this->m_lock.unlock();

		std::sort(std::begin(data), std::end(data), [](const MessagePair& x, const MessagePair& y)
		{
			return x.second.GetObjectId().compare(y.second.GetObjectId()) < 0;
		});
		
		std::vector<models::RawMeasurement> authorized;
		authorized.reserve(data.size());

		for(auto&& pair : data) {
			if(!sensor.second.has_value() || sensor.second->GetId() != pair.second.GetObjectId()) {
				sensor = this->m_cache->GetSensor(pair.second.GetObjectId());
			}

			if(!sensor.first) {
				if(!this->m_cache->IsBlackListed(pair.second.GetObjectId())) {
					/* Add to not found list & continue */
					notFound.push_back(pair.second.GetObjectId());
					leftOver.emplace_back(std::forward<MessagePair>(pair));
				}

				continue;
			}

			if(!sensor.second.has_value()) {
				/* Found but not valid, exit */
				continue;
			}

			/* Valid sensor, validate the measurement */
			if(!this->ValidateMeasurement(sensor.second.value(), pair)) {
				continue;
			}

			authorized.emplace_back(std::move(pair.second));
		}

		data.clear();

		if(!authorized.empty()) {
			this->PublishAuthorizedMessages(authorized);
		}

		if(!leftOver.empty()) {
			std::stringstream stream;

			stream << "Unable to process " << leftOver.size() << " measurements.";
			log << stream.str() << util::Log::NewLine;
		}

		std::scoped_lock l(this->m_lock);
		this->m_leftOver = std::move(leftOver);

		return std::make_pair(authorized.size(), std::move(notFound));
	}
}
