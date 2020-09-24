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

#include <mqtt/async_client.h>

namespace sensateiot::consumers
{
	MeasurementConsumer::MeasurementConsumer(mqtt::IMqttClient &client, data::DataCache &cache, config::Config conf) :
		AbstractConsumer(client, cache, std::move(conf))
	{
	}

	MeasurementConsumer::~MeasurementConsumer()
	{
		std::scoped_lock l(this->m_lock);
		this->m_leftOver.clear();
	}

	MeasurementConsumer::MeasurementConsumer(MeasurementConsumer &&rhs) noexcept :
		AbstractConsumer(std::forward<AbstractConsumer>(rhs))
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);
		this->m_leftOver = std::move(rhs.m_leftOver);
	}

	MeasurementConsumer &MeasurementConsumer::operator=(MeasurementConsumer &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_leftOver = std::move(rhs.m_leftOver);
		this->Move(rhs);

		return *this;
	}

	bool MeasurementConsumer::ValidateMeasurement(const models::Sensor& sensor, MessagePair & pair) const
	{
		auto result = RE2::Replace(&pair.first, this->m_regex, sensor.GetSecret());

		if(result) {
			auto offset = pair.second.GetKey().length() - SecretSubStringOffset;
			auto key = pair.second.GetKey().substr(SecretSubstringStart, offset);
			return HashCompare(pair.first, key);
		}

		/* This is not a SHA256 secured message. Authorize manually. */
		return pair.second.GetKey() == sensor.GetSecret();
	}

	MeasurementConsumer::ProcessingStats MeasurementConsumer::Process()
	{
		std::vector<MessagePair> data;
		std::vector<MessagePair> leftOver;
		std::vector<models::ObjectId> notFound;
		SensorLookupType sensor;

		this->m_lock.lock();
		data.reserve(MessageArraySize);
		std::swap(this->m_messages, data);
		this->m_messages.clear();
		this->m_lock.unlock();

		std::sort(std::begin(data), std::end(data), [](const MessagePair& x, const MessagePair& y)
		{
			return x.second.GetObjectId().compare(y.second.GetObjectId()) < 0;
		});
		
		std::vector<models::Measurement> authorized;
		authorized.reserve(data.size());
		auto now = std::chrono::high_resolution_clock::now();

		for(auto&& pair : data) {
			if(!sensor.second.has_value() || sensor.second->GetId() != pair.second.GetObjectId()) {
				sensor = this->m_cache->GetSensor(pair.second.GetObjectId(), now);
			}

			if(!sensor.first) {
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
			this->PublishAuthorizedMessages(authorized, this->m_config.GetMqtt().GetPrivateBroker().GetBulkMeasurementTopic());
		}

		std::scoped_lock l(this->m_lock);
		this->m_leftOver = std::move(leftOver);

		return std::make_pair(authorized.size(), std::move(notFound));
	}
}
