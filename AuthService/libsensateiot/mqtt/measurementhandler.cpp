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
	MeasurementHandler::MeasurementHandler(IMqttClient &client) :
		m_internal(client)
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
		std::scoped_lock l(this->m_lock);

		data.reserve(this->m_measurements.size());
		std::swap(this->m_measurements, data);
		this->m_measurements.clear();

//		auto& log = util::Log::GetLog();

//		log << "Processing: " << std::to_string(data.size()) << util::Log::NewLine;
		return std::vector<models::ObjectId>();
	}
}
