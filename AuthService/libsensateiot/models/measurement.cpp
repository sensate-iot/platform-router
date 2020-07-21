/*
 * Raw measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/models/measurement.h>

namespace sensateiot::models
{
	void Measurement::SetObjectId(const ObjectId &id)
	{
		this->m_id = id;
	}

	const ObjectId &Measurement::SetObjectId()
	{
		return this->m_id;
	}

	const ObjectId &Measurement::GetObjectId() const
	{
		return this->m_id;
	}

	void Measurement::SetKey(const std::string &key)
	{
		this->m_key = key;
	}

	const std::string &Measurement::GetKey() const
	{
		return this->m_key;
	}

	void Measurement::SetCreatedTimestamp(const std::string &timestamp)
	{
		this->m_createdAt = timestamp;
	}

	const std::string &Measurement::GetCreatedTimestamp() const
	{
		return this->m_createdAt;
	}

	void Measurement::SetCoordinates(double lon, double lat)
	{
		this->m_coords = std::make_pair(lon, lat);
	}

	const std::pair<double, double>& Measurement::GetCoordinates() const
	{
		return this->m_coords;
	}

	void Measurement::SetData(std::vector<DataEntry>&& data)
	{
		this->m_data = std::forward<std::vector<DataEntry>>(data);
	}

	const std::vector<Measurement::DataEntry> &Measurement::GetData() const
	{
		return this->m_data;
	}
}
