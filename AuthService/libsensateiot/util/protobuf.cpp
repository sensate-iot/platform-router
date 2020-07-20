/*
 * JSON serialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/protobuf.h>
#include <sensateiot/util/base64.h>
#include <sensateiot/util/time.h>

#include <proto/datapoint.pb.h>

#include <string>
#include <limits>

namespace sensateiot::util
{
	template <>
	std::string to_protobuf<std::vector<models::RawMeasurement>>(const std::vector<models::RawMeasurement>& value)
	{
		return to_protobuf(value.begin(), value.end());
	}

	std::string to_protobuf(
		std::vector<models::RawMeasurement>::const_iterator begin,
		std::vector<models::RawMeasurement>::const_iterator end)
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

			if (entry.GetCreatedTimestamp().empty()) {
				measurement->set_timestamp(now);
			}
			else {
				measurement->set_timestamp(entry.GetCreatedTimestamp());
			}
		}

		std::vector<std::uint8_t> bytes( data.ByteSizeLong());

		if (bytes.size() > std::numeric_limits<int>::max()) {
			throw std::out_of_range("Serialization length to large!");
		}

		return data.SerializeAsString();
		/*data.SerializeToArray(bytes.data(), static_cast<int>(bytes.size()));
		auto encoded = Encode64(bytes);

		std::cout << "Byte size: " << data.ByteSizeLong() << std::endl;
		std::cout << "Encoded size: " << encoded.size() << std::endl;
		return encoded;*/
	}
}
