/*
 * Raw measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/models/objectid.h>

#include <utility>
#include <optional>

#include <json.hpp>

namespace sensateiot::models
{
	struct Measurement {
		struct DataEntry {
			std::string m_unit;
			double m_value;
			std::optional<double> m_accuracy;
			std::optional<double> m_precision;
			std::string m_key;
		};

		void SetObjectId(const ObjectId& id);
		const ObjectId& SetObjectId();
		[[nodiscard]] const ObjectId& GetObjectId() const;

		void SetKey(const std::string& key);
		[[nodiscard]] const std::string& GetKey() const;

		void SetCreatedTimestamp(const std::string& timestamp);
		[[nodiscard]] const std::string& GetCreatedTimestamp() const;

		void SetCoordinates(double lon, double lat);
		[[nodiscard]] const std::pair<double, double>& GetCoordinates() const;

		void SetData(std::vector<DataEntry>&& data);
		[[nodiscard]] const std::vector<DataEntry>& GetData() const;

		static constexpr std::string_view DataValue = std::string_view("value");
		static constexpr std::string_view DataUnit = std::string_view("unit");
		static constexpr std::string_view DataPrecision = std::string_view("precision");
		static constexpr std::string_view DataAccuracy = std::string_view("accuracy");

		static constexpr std::string_view Longitude = std::string_view("longitude");
		static constexpr std::string_view Latitude = std::string_view("latitude");
		static constexpr std::string_view Timestamp = std::string_view("createdAt");

	private:
		ObjectId m_id;
		std::string m_key;
		std::string m_createdAt;

		std::vector<DataEntry> m_data;
		std::pair<double, double> m_coords;
	};
}
