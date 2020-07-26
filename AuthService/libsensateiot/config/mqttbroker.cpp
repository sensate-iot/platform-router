/*
 * Sensate IoT configuration file.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/mqttbroker.h>

namespace sensateiot::config
{
	// Generic broker

	const std::string &MqttBroker::GetHostName() const
	{
		return this->m_host;
	}

	void MqttBroker::SetHostName(const std::string &host)
	{
		this->m_host = host;
	}

	const std::string &MqttBroker::GetUsername() const
	{
		return this->m_username;
	}

	void MqttBroker::SetUsername(const std::string &username)
	{
		this->m_username = username;
	}

	const std::string &MqttBroker::GetPassword() const
	{
		return this->m_password;
	}

	void MqttBroker::SetPassword(const std::string &password)
	{
		this->m_password = password;
	}

	std::uint16_t MqttBroker::GetPort() const
	{
		return this->m_port;
	}

	void MqttBroker::SetPortNumber(std::uint16_t port)
	{
		this->m_port = port;
	}

	bool MqttBroker::GetSsl() const
	{
		return this->m_ssl;
	}

	void MqttBroker::SetSsl(bool enable)
	{
		this->m_ssl = enable;
	}

	std::string MqttBroker::GetUri() const
	{
		std::string rv = this->GetHostName() + ':';
		rv += std::to_string(this->GetPort());

		return rv;
	}

	// Public broker

	const std::string &PublicBroker::GetMeasurementTopic() const
	{
		return this->m_measurementTopic;
	}

	void PublicBroker::SetMeasurementTopic(const std::string &topic)
	{
		this->m_measurementTopic = topic;
	}

	const std::string &PublicBroker::GetBulkMeasurementTopic() const
	{
		return this->m_bulkMeasurementTopic;
	}

	void PublicBroker::SetBulkMeasurementTopic(const std::string &topic)
	{
		this->m_bulkMeasurementTopic = topic;
	}

	MqttBroker &PublicBroker::GetBroker()
	{
		return this->m_broker;
	}

	const MqttBroker& PublicBroker::GetBroker() const
	{
		return this->m_broker;
	}

	const std::string &PublicBroker::GetMessageTopic() const
	{
		return this->m_messageTopic;
	}

	void PublicBroker::SetMessageTopic(const std::string &topic)
	{
		this->m_messageTopic = topic;
	}

	// Private broker

	const std::string &PrivateBroker::GetBulkMeasurementTopic() const
	{
		return this->m_internalBulkMeasurementTopic;
	}

	void PrivateBroker::SetBulkMeasurementTopic(const std::string &topic)
	{
		this->m_internalBulkMeasurementTopic = topic;
	}

	const std::string& PrivateBroker::GetClientId() const
	{
		return this->m_clientId;
	}

	void PrivateBroker::SetClientId(const std::string& id)
	{
		this->m_clientId = id;
	}

	const MqttBroker& PrivateBroker::GetBroker() const
	{
		return this->m_broker;
	}

	MqttBroker &PrivateBroker::GetBroker()
	{
		return this->m_broker;
	}

	const std::string &PrivateBroker::GetBulkMessageTopic() const
	{
		return this->m_internalBulkMessageTopic;
	}

	void PrivateBroker::SetBulkMessageTopic(const std::string &topic)
	{
		this->m_internalBulkMessageTopic = topic;
	}
}
