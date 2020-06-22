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

	MeasurementHandler &MeasurementHandler::operator=(MeasurementHandler &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
		return *this;
	}

	MeasurementHandler::MeasurementHandler(MeasurementHandler &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
	}

	void MeasurementHandler::PushMeasurement(MeasurementPair measurement)
	{
		std::scoped_lock l(this->m_lock);
		this->m_measurements.emplace_back(std::move(measurement));
	}

	std::vector<models::ObjectId> MeasurementHandler::Process()
	{
		std::vector<MeasurementPair> data;
		std::vector<models::ObjectId> notFound;
		std::pair<bool, std::optional<models::Sensor>> sensor;

		std::scoped_lock l(this->m_lock);

		data.reserve(this->m_measurements.size());
		std::swap(this->m_measurements, data);
		this->m_measurements.clear();

		/* Optimize by sorting vector by sensor */

		for(auto&& pair : this->m_measurements) {
//			if(sensor.GetId() != pair.second.GetObjectId()) {
			if(!sensor.second.has_value() || sensor.second->GetId().compare(pair.second.GetObjectId()) == 0) {
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

			/* Valid sensor, user and key for current measurement */
		}

//		auto& log = util::Log::GetLog();

//		log << "Processing: " << std::to_string(data.size()) << util::Log::NewLine;
		return notFound;
	}
}
