
#pragma once

#include <rapidjson/document.h>
#include <rapidjson/rapidjson.h>
#include <sensateiot/models/measurement.h>

#include <string>
#include <vector>
#include <stdexcept>

namespace sensateiot::detail
{
	template <typename T>
	std::pair<bool, models::Measurement> ParseSingleMeasurement(const T& json)
	{
		try {
			std::vector<models::Measurement::DataEntry> entries;

			models::Measurement raw;
			models::ObjectId id(json["createdById"].GetString());

			raw.SetKey(json["createdBySecret"].GetString());
			raw.SetObjectId(id);

			if (json.HasMember(models::Measurement::Longitude.data()) &&
				json.HasMember(models::Measurement::Latitude.data())) {
				raw.SetCoordinates(json[models::Measurement::Longitude.data()].GetDouble(),
					json[models::Measurement::Latitude.data()].GetDouble());
			}

			if (json.HasMember(models::Measurement::Timestamp.data())) {
				raw.SetCreatedTimestamp(json[models::Measurement::Timestamp.data()].GetString());
			}

			auto end = json["data"].MemberEnd();
			for (auto it = json["data"].MemberBegin(); it != end; ++it) {
				models::Measurement::DataEntry entry;
				auto& value = it->value;

				entry.m_key = it->name.GetString();
				entry.m_value = value[models::Measurement::DataValue.data()].GetDouble();

				if (value.HasMember(models::Measurement::DataUnit.data())) {
					entry.m_unit = value[models::Measurement::DataUnit.data()].GetString();
				}

				if (value.HasMember(models::Measurement::DataAccuracy.data())) {
					entry.m_accuracy = value[models::Measurement::DataAccuracy.data()].GetDouble();
				}

				if (value.HasMember(models::Measurement::DataPrecision.data())) {
					entry.m_precision = value[models::Measurement::DataPrecision.data()].GetDouble();
				}

				entries.emplace_back(std::move(entry));
			}

			raw.SetData(std::move(entries));
			return std::make_pair(true, std::move(raw));
		}
		catch (std::exception&) {
			return std::make_pair(false, models::Measurement());
		}
	}
}
