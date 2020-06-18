/*
 * JSON measurement validator implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/measurementvalidator.h>

#include <json.hpp>
#include <iostream>

namespace nlohmann
{
	bool exists(const json &j, const char *key)
	{
		return j.find(key) != j.end();
	}

	bool exists(const json &j, const std::string_view &key)
	{
		return exists(j, key.data());
	}
}

namespace sensateiot::data
{
	std::pair<bool, models::RawMeasurement> MeasurementValidator::operator()(const std::string &str)
	{
		try {
			auto json = nlohmann::json::parse(str);
			models::RawMeasurement raw;
			models::ObjectId id(json["createdById"].get<std::string>());
			std::vector<models::RawMeasurement::DataEntry> entries;

			raw.SetKey(json["createdBySecret"].get<std::string>());
			raw.SetObjectId(id);

			if(nlohmann::exists(json, models::RawMeasurement::Longitude) &&
			   nlohmann::exists(json, models::RawMeasurement::Latitude)) {
				raw.SetCoordinates(json[models::RawMeasurement::Longitude.data()],
				                   json[models::RawMeasurement::Latitude.data()]);
			}

			if(nlohmann::exists(json, models::RawMeasurement::Timestamp)) {
				raw.SetCreatedTimestamp(json[models::RawMeasurement::Timestamp.data()]);
			}

			for(auto &item : json["data"].items()) {
				models::RawMeasurement::DataEntry entry;
				auto &value = item.value();

				entry.m_key = item.key();
				entry.m_value = value[models::RawMeasurement::DataValue.data()];

				if(nlohmann::exists(value, models::RawMeasurement::DataAccuracy)) {
					entry.m_accuracy = value[models::RawMeasurement::DataAccuracy.data()];
				}

				if(nlohmann::exists(value, models::RawMeasurement::DataPrecision)) {
					entry.m_precision = value[models::RawMeasurement::DataPrecision.data()];
				}

				if(nlohmann::exists(value, models::RawMeasurement::DataUnit)) {
					entry.m_unit = value[models::RawMeasurement::DataUnit.data()];
				}

				entries.emplace_back(std::move(entry));
			}

			raw.SetData(std::move(entries));
			return std::make_pair(true, raw);
		} catch(std::exception &ex) {
			std::cout << "Error: " << ex.what() << std::endl;
		}

		return std::make_pair(false, models::RawMeasurement());
	}
}
