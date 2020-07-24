
#pragma once

#include <sensateiot/models/measurement.h>
#include <sensateiot/models/message.h>
#include <sensateiot/util/time.h>

#include <string>
#include <vector>
#include <stdexcept>

namespace sensateiot::detail
{
	template <typename T>
	static bool HasValidCoordinates(const T& json)
	{
		return json.HasMember(models::Measurement::Longitude.data()) &&
			json.HasMember(models::Measurement::Latitude.data()) &&
			json[models::Measurement::Longitude.data()].IsDouble() &&
			json[models::Measurement::Latitude.data()].IsDouble();
	}

	template <typename T>
	std::pair<bool, models::Message> ParseSingleMessage(const T& json)
	{
		try {
			models::Message msg;
			models::ObjectId id;

			if(json.HasMember(models::Message::SensorId.data()) && json[models::Message::SensorId.data()].IsString()) {
				id = models::ObjectId(json[models::Message::SensorId.data()].GetString());
				msg.SetObjectId(id);
			} else {
				return {};
			}

			if(HasValidCoordinates(json)) {
				msg.SetLocation({ json[models::Message::Longitude.data()].GetDouble(),
					json[models::Message::Latitude.data()].GetDouble() });
			}
			
			if (json.HasMember(models::Message::Timestamp.data()) && json[models::Message::Timestamp.data()].IsString()) {
				msg.SetCreatedAt(json[models::Message::Timestamp.data()].GetString());
			} else {
				auto now = util::GetIsoTimestamp();
				msg.SetCreatedAt(std::move(now));
			}

			if(json.HasMember(models::Message::Data.data()) && json[models::Message::Data.data()].IsString()) {
				msg.SetData(json[models::Message::Data.data()].GetString());
			} else {
				return {};
			}
			
			if(json.HasMember(models::Message::Secret.data()) && json[models::Message::Secret.data()].IsString()) {
				msg.SetSecret(json[models::Message::Secret.data()].GetString());
			} else {
				return {};
			}

			return std::make_pair(true, std::move(msg));
		} catch(std::exception&) {
			return {};
		}
	}

	template <typename T>
	std::pair<bool, models::Measurement> ParseSingleMeasurement(const T& json)
	{
		try {
			std::vector<models::Measurement::DataEntry> entries;

			models::Measurement raw;

			if(json.HasMember(models::Measurement::CreatedBy.data()) && json[models::Measurement::CreatedBy.data()].IsString()) {
				models::ObjectId id(json[models::Measurement::CreatedBy.data()].GetString());
				raw.SetObjectId(id);
			} else {
				return {};
			}

			if(json.HasMember(models::Measurement::SensorSecret.data()) && json[models::Measurement::SensorSecret.data()].IsString()) {
				raw.SetKey(json[models::Message::Secret.data()].GetString());
			} else {
				return {};
			}

			if(HasValidCoordinates(json)) {
				raw.SetCoordinates(json[models::Measurement::Longitude.data()].GetDouble(),
					json[models::Measurement::Latitude.data()].GetDouble());
			}

			if (json.HasMember(models::Measurement::Timestamp.data()) &&
				json[models::Measurement::Timestamp.data()].IsString()) {
				raw.SetCreatedTimestamp(json[models::Measurement::Timestamp.data()].GetString());
			}

			if(!(json.HasMember(models::Measurement::Data.data()) &&
				json[models::Measurement::Data.data()].IsObject())) {
				return {};
			}

			auto end = json[models::Measurement::Data.data()].MemberEnd();
			for (auto it = json[models::Measurement::Data.data()].MemberBegin(); it != end; ++it) {
				models::Measurement::DataEntry entry;
				auto& value = it->value;

				entry.m_key = it->name.GetString();

				if (value.HasMember(models::Measurement::DataValue.data()) &&
					value[models::Measurement::DataValue.data()].IsDouble()) {
					entry.m_value = value[models::Measurement::DataValue.data()].GetDouble();
				} else {
					return {};
				}

				if (value.HasMember(models::Measurement::DataUnit.data()) && value[models::Measurement::DataUnit.data()].IsString()) {
					entry.m_unit = value[models::Measurement::DataUnit.data()].GetString();
				}

				if (value.HasMember(models::Measurement::DataAccuracy.data()) && value[models::Measurement::DataAccuracy.data()].IsDouble()) {
					entry.m_accuracy = value[models::Measurement::DataAccuracy.data()].GetDouble();
				}

				if (value.HasMember(models::Measurement::DataPrecision.data()) && value[models::Measurement::DataPrecision.data()].IsDouble()) {
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
