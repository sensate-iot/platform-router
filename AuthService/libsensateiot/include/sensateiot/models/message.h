/*
 * Message model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <sensateiot/models/objectid.h>

#include <utility>
#include <optional>
#include <string>
#include <string_view>

namespace sensateiot::models
{
	class Message {
	public:
		void SetSecret(const std::string& secret);
		[[nodiscard]] const std::string& GetSecret() const;
		
		void SetData(const std::string& data);
		[[nodiscard]] const std::string& GetData() const;

		void SetCreatedAt(const std::string& date);
		[[nodiscard]] const std::string& GetCreatedAt() const;
		
		void SetObjectId(const ObjectId& id);
		[[nodiscard]] const ObjectId& GetObjectId() const;
		
		void SetLocation(std::pair<double,double> location);
		[[nodiscard]] const std::optional<std::pair<double,double>>& GetLocation() const;

	private:
		std::string m_secret;
		std::string m_data;
		std::string m_createdAt;
		ObjectId m_sensorId;
		std::optional<std::pair<double, double>> m_coords;

	public:
		static constexpr auto Secret = std::string_view("secret");
		static constexpr auto Data = std::string_view("data");
		static constexpr auto Timestamp = std::string_view("timestamp");
		static constexpr auto SensorId = std::string_view("sensorId");
		static constexpr auto Longitude = std::string_view("longitude");
		static constexpr auto Latitude = std::string_view("latitude");
	};
}
