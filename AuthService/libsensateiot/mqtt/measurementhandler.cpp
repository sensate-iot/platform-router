/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <iostream>

#include <sensateiot/mqtt/measurementhandler.h>
#include <sensateiot/util/log.h>
#include <sensateiot/util/sha256.h>
#include <sensateiot/util/protobuf.h>

namespace sensateiot::mqtt
{
	MeasurementHandler::MeasurementHandler(IMqttClient &client, data::DataCache &cache) : m_internal(client),
	                                                                                      m_cache(cache),
	                                                                                      m_regex(SearchRegex.data())
	{

	}

	MeasurementHandler::~MeasurementHandler()
	{
		this->m_lock.lock();
		this->m_measurements.clear();
		this->m_lock.unlock();
	}

	MeasurementHandler::MeasurementHandler(MeasurementHandler &&rhs) noexcept : m_regex(SearchRegex.data())
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
		this->m_cache = std::move(rhs.m_cache);
		this->m_leftOver = std::move(rhs.m_leftOver);
	}

	MeasurementHandler &MeasurementHandler::operator=(MeasurementHandler &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
		this->m_cache = std::move(rhs.m_cache);
		this->m_leftOver = std::move(rhs.m_leftOver);
		return *this;
	}

	bool MeasurementHandler::ValidateMeasurement(const models::Sensor& sensor, MeasurementPair & pair) const
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

	void MeasurementHandler::PushMeasurement(MeasurementPair measurement)
	{
		std::scoped_lock l(this->m_lock);
		this->m_measurements.emplace_back(std::move(measurement));
	}

	void MeasurementHandler::ProcessLeftOvers()
	{
		std::vector<MeasurementPair> data;
		SensorLookupType sensor;

		this->m_lock.lock();

		if(this->m_leftOver.empty()) {
			this->m_lock.unlock();
			return;
		}
		
		data.reserve(this->m_leftOver.size());
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
			return;
		}

		auto json = util::to_protobuf(authorized);
		//std::cout << json << std::endl;
		(void)json;
	}

	std::vector<models::ObjectId> MeasurementHandler::Process()
	{
		this->ProcessLeftOvers();
		
		std::vector<MeasurementPair> data;
		std::vector<MeasurementPair> leftOver;
		std::vector<models::ObjectId> notFound;
		SensorLookupType sensor;
		auto& log = util::Log::GetLog();

		this->m_lock.lock();
		data.reserve(this->m_measurements.size());
		std::swap(this->m_measurements, data);
		this->m_measurements.clear();
		this->m_lock.unlock();

		std::sort(std::begin(data), std::end(data), [](const MeasurementPair& x, const MeasurementPair& y)
		{
			return x.second.GetObjectId().compare(y.second.GetObjectId()) < 0;
		});
		
		std::vector<MeasurementPair> authorized;
		authorized.reserve(data.size());

		for(auto&& pair : data) {
			if(!sensor.second.has_value() || sensor.second->GetId() != pair.second.GetObjectId()) {
				sensor = this->m_cache->GetSensor(pair.second.GetObjectId());
			}

			if(!sensor.first) {
				if(!this->m_cache->IsBlackListed(pair.second.GetObjectId())) {
					/* Add to not found list & continue */
					notFound.push_back(pair.second.GetObjectId());
					leftOver.emplace_back(std::forward<MeasurementPair>(pair));
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

			authorized.emplace_back(std::move(pair));
		}

		if(!authorized.empty()) {
			auto buf = util::to_protobuf(authorized);
			(void)buf;
		}

		std::stringstream stream;

		stream << "Unable to process " << leftOver.size() << " measurements.";
		log << stream.str() << util::Log::NewLine;

		std::scoped_lock l(this->m_lock);
		this->m_leftOver = std::move(leftOver);

		return notFound;
	}
}
