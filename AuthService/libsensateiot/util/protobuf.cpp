/*
 * JSON serialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/protobuf.h>
#include <sensateiot/util/base64.h>
#include <sensateiot/util/time.h>

#include <proto/measurement.pb.h>
#include <proto/message.pb.h>

#include <string>
#include <limits>

namespace sensateiot::util
{
	template <>
	std::vector<char> to_protobuf<std::vector<models::Measurement>>(const std::vector<models::Measurement>& value)
	{
		return to_protobuf(value.begin(), value.end());
	}
	
	template <>
	std::vector<char> to_protobuf<std::vector<models::Message>>(const std::vector<models::Message>& value)
	{
		return to_protobuf(value.begin(), value.end());
	}
	
	std::vector<char> to_protobuf(
		std::vector<models::Message>::const_iterator begin,
		std::vector<models::Message>::const_iterator end)
	{
		TextMessageData data;

		for(auto it = begin; it != end; ++it) {
			auto* message = data.add_messages();
			const auto& entry = *it;
			const auto& location = entry.GetLocation();

			if(location.has_value()) {
				message->set_longitude(location->first);
				message->set_latitude(location->second);
			}

			message->set_data(entry.GetData());
			message->set_created_at(entry.GetCreatedAt());
			message->set_sensor_id(entry.GetObjectId().ToString());
		}

		std::vector<char> bytes( data.ByteSizeLong());

		if (bytes.size() > std::numeric_limits<int>::max()) {
			throw std::out_of_range("Serialization length to large!");
		}

		data.SerializeToArray(bytes.data(), static_cast<int>(bytes.size()));
		return bytes;
	}

	std::vector<char> to_protobuf(
		std::vector<models::Measurement>::const_iterator begin,
		std::vector<models::Measurement>::const_iterator end)
	{
		MeasurementData data;
		auto now = util::GetIsoTimestamp();

		for(auto it = begin; it != end; ++it) {
			auto* measurement = data.add_measurements();
			const auto& entry = *it;
			
			for(const auto& dp : entry.GetData()) {
				auto* datapoint = measurement->add_datapoints();

				datapoint->set_value(dp.m_value);
				datapoint->set_unit(dp.m_unit);
				datapoint->set_key(dp.m_key);

				if(dp.m_accuracy.has_value()) {
					datapoint->set_accuracy(dp.m_accuracy.value());
				}
				
				if(dp.m_precision.has_value()) {
					datapoint->set_accuracy(dp.m_precision.value());
				}
			}

			measurement->set_latitude(entry.GetCoordinates().first);
			measurement->set_longitude(entry.GetCoordinates().second);
			measurement->set_platformtime(now);
			measurement->set_sensor_id(entry.GetObjectId().ToString());

			if (entry.GetCreatedTimestamp().empty()) {
				measurement->set_timestamp(now);
			}
			else {
				measurement->set_timestamp(entry.GetCreatedTimestamp());
			}
		}

		std::vector<char> bytes( data.ByteSizeLong());

		if (bytes.size() > std::numeric_limits<int>::max()) {
			throw std::out_of_range("Serialization length to large!");
		}

		data.SerializeToArray(bytes.data(), static_cast<int>(bytes.size()));
		return bytes;
	}
}
