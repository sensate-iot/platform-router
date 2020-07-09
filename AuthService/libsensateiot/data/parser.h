
#pragma once

#include <rapidjson/document.h>
#include <rapidjson/rapidjson.h>
#include <sensateiot/models/rawmeasurement.h>

#include <string>
#include <vector>
#include <stdexcept>

namespace sensateiot::detail
{
	template <typename T>
	std::pair<bool, models::RawMeasurement> ParseSingleMeasurement(const T& json)
	{
		try {
			std::vector<models::RawMeasurement::DataEntry> entries;

			models::RawMeasurement raw;
			models::ObjectId id(json["createdById"].GetString());

			raw.SetKey(json["createdBySecret"].GetString());
			raw.SetObjectId(id);

			if (json.HasMember(models::RawMeasurement::Longitude.data()) &&
				json.HasMember(models::RawMeasurement::Latitude.data())) {
				raw.SetCoordinates(json[models::RawMeasurement::Longitude].GetDouble(),
					json[models::RawMeasurement::Latitude].GetDouble());
			}

			if (json.HasMember(models::RawMeasurement::Timestamp.data())) {
				raw.SetCreatedTimestamp(json[models::RawMeasurement::Timestamp].GetString());
			}

			auto end = json["data"].MemberEnd();
			for (auto it = json["data"].MemberBegin(); it != end; ++it) {
				models::RawMeasurement::DataEntry entry;
				auto& value = it->value;

				entry.m_key = it->name.GetString();
				entry.m_value = value[models::RawMeasurement::DataValue].GetDouble();

				if (value.HasMember(models::RawMeasurement::DataUnit.data())) {
					entry.m_unit = value[models::RawMeasurement::DataUnit].GetString();
				}

				if (value.HasMember(models::RawMeasurement::DataAccuracy.data())) {
					entry.m_accuracy = value[models::RawMeasurement::DataAccuracy].GetDouble();
				}

				if (value.HasMember(models::RawMeasurement::DataPrecision.data())) {
					entry.m_precision = value[models::RawMeasurement::DataPrecision].GetDouble();
				}

				entries.emplace_back(std::move(entry));
			}

			raw.SetData(std::move(entries));
			return std::make_pair(true, std::move(raw));
		}
		catch (std::exception&) {
			return std::make_pair(false, models::RawMeasurement());
		}
	}
}
