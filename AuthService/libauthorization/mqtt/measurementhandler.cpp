/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/measurementhandler.h>
#include <mutex>

namespace sensateiot::mqtt
{
	MeasurementHandler::MeasurementHandler(InternalMqttClient &client) :
		m_internal(client)
	{

	}

	MeasurementHandler::~MeasurementHandler()
	{

	}

	MeasurementHandler &MeasurementHandler::operator=(MeasurementHandler &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);


		return *this;
	}

	MeasurementHandler::MeasurementHandler(MeasurementHandler &&rhs) noexcept
	{
		std::scoped_lock l(this->m_lock, rhs.m_lock);

		this->m_internal = std::move(rhs.m_internal);
		this->m_measurements = std::move(rhs.m_measurements);
	}
}
