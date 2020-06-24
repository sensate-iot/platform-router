/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/mqtt/measurementhandler.h>

#include <iostream>
#include <sensateiot/util/log.h>

namespace sensateiot::mqtt
{
	MeasurementHandler::MeasurementHandler(IMqttClient &client, data::DataCache &cache) : m_internal(client),
	                                                                                      m_cache(cache)
	{

	}

	MeasurementHandler::~MeasurementHandler()
	{
		this->m_lock.lock();
		this->m_measurements.clear();
		this->m_lock.unlock();
	}

	re2::RE2 MeasurementHandler::SearchRegex = re2::RE2("\\$[a-f0-9]{64}==");

	MeasurementHandler::MeasurementHandler(MeasurementHandler &&rhs) noexcept
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

	bool MeasurementHandler::ValidateMeasurement(const models::Sensor& sensor, const MeasurementPair & pair) const
	{
		return false;
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
		data.reserve(this->m_leftOver.size());
		std::swap(this->m_leftOver, data);
		this->m_leftOver.clear();
		this->m_lock.unlock();

		std::sort(std::begin(data), std::end(data), [](const MeasurementPair& x, const MeasurementPair& y)
		{
			return x.second.GetObjectId().compare(y.second.GetObjectId()) < 0;
		});

		for(auto& pair : data) {
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
		}
	}

	std::vector<models::ObjectId> MeasurementHandler::Process()
	{
		std::vector<MeasurementPair> data;
		std::vector<models::ObjectId> notFound;
		SensorLookupType sensor;

		this->m_lock.lock();

		data.reserve(this->m_measurements.size());
		std::swap(this->m_measurements, data);
		this->m_measurements.clear();

		this->m_lock.unlock();

		/* TODO: Optimize by sorting vector by sensor */
		std::sort(std::begin(data), std::end(data), [](const MeasurementPair& x, const MeasurementPair& y)
		{
			return x.second.GetObjectId().compare(y.second.GetObjectId()) < 0;
		});

		for(auto&& pair : data) {
//			if(sensor.GetId() != pair.second.GetObjectId()) {
			if(!sensor.second.has_value() || sensor.second->GetId() != pair.second.GetObjectId()) {
				sensor = this->m_cache->GetSensor(pair.second.GetObjectId());
			}

			if(!sensor.first) {
				/* Add to not found list & continue */
				notFound.push_back(pair.second.GetObjectId());
				this->m_leftOver.emplace_back(std::forward<MeasurementPair>(pair));
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
		}

//		auto& log = util::Log::GetLog();

//		log << "Processing: " << std::to_string(data.size()) << util::Log::NewLine;
		return notFound;
	}
}
