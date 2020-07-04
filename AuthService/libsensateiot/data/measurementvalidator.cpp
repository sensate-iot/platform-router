/*
 * JSON measurement validator implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/data/measurementvalidator.h>

#include <json.hpp>

#include <rapidjson/document.h>
#include <rapidjson/rapidjson.h>

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
	std::pair<bool, models::RawMeasurement> MeasurementValidator::operator()(const std::string &str) const
	{
		try {
			//auto json = nlohmann::json::parse(str);
			rapidjson::Document json;
			json.Parse(str.c_str());
			std::vector<models::RawMeasurement::DataEntry> entries;

			models::RawMeasurement raw;
			models::ObjectId id(json["createdById"].GetString());

			raw.SetKey(json["createdBySecret"].GetString());
			raw.SetObjectId(id);

			if (json.HasMember(models::RawMeasurement::Longitude.data()) &&
				json.HasMember(models::RawMeasurement::Latitude.data())) {
				raw.SetCoordinates(json[models::RawMeasurement::Longitude.data()].GetDouble(),
					json[models::RawMeasurement::Latitude.data()].GetDouble());
			}

			if (json.HasMember(models::RawMeasurement::Timestamp.data())) {
				raw.SetCreatedTimestamp(json[models::RawMeasurement::Timestamp.data()].GetString());
			}

			/*models::ObjectId id(json["createdById"].get<std::string>());
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
			}*/

			auto end = json["data"].MemberEnd();
			for (auto it = json["data"].MemberBegin(); it != end; ++it) {
				models::RawMeasurement::DataEntry entry;
				auto& value = it->value;

				entry.m_key = it->name.GetString();
				entry.m_value = value[models::RawMeasurement::DataValue.data()].GetDouble();

				if (value.HasMember(models::RawMeasurement::DataUnit.data())) {
					entry.m_unit = value[models::RawMeasurement::DataUnit.data()].GetString();
				}

				if (value.HasMember(models::RawMeasurement::DataAccuracy.data())) {
					entry.m_accuracy = value[models::RawMeasurement::DataAccuracy.data()].GetDouble();
				}

				if (value.HasMember(models::RawMeasurement::DataPrecision.data())) {
					entry.m_precision = value[models::RawMeasurement::DataPrecision.data()].GetDouble();
				}

				entries.emplace_back(std::move(entry));
			}


			//for(auto &value : json["data"].GetArray()) {
				/*models::RawMeasurement::DataEntry entry;

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

				*/
			//}

			raw.SetData(std::move(entries));
			return std::make_pair(true, std::move(raw));
		} catch(std::exception &ex) {
			//std::cout << "Error: " << ex.what() << std::endl;
		}

		return std::make_pair(false, models::RawMeasurement());
	}
}
