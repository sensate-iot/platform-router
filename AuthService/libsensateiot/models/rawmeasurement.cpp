/*
 * Raw measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/rawmeasurement.h>

namespace sensateiot::models
{
	void RawMeasurement::SetObjectId(const ObjectId &id)
	{
		this->m_id = id;
	}

	const ObjectId &RawMeasurement::SetObjectId()
	{
		return this->m_id;
	}

	void RawMeasurement::SetKey(const std::string &key)
	{
		this->m_key = key;
	}

	const std::string &RawMeasurement::GetKey() const
	{
		return this->m_key;
	}

	void RawMeasurement::SetCreatedTimestamp(const std::string &timestamp)
	{
		this->m_createdAt = timestamp;
	}

	const std::string &RawMeasurement::GetCreatedTimestamp() const
	{
		return this->m_createdAt;
	}

	void RawMeasurement::SetCoordinates(double lon, double lat)
	{
		this->m_coords = std::make_pair(lon, lat);
	}

	std::pair<double, double> RawMeasurement::GetCoordinates() const
	{
		return this->m_coords;
	}

	void RawMeasurement::SetData(std::vector<DataEntry>&& data)
	{
		this->m_data = std::forward<std::vector<DataEntry>>(data);
	}

	const std::vector<RawMeasurement::DataEntry> &RawMeasurement::GetData() const
	{
		return this->m_data;
	}
}
