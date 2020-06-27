/*
 * JSON serialization.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/protobuf.h>
#include <sensateiot/util/base64.h>

#include <proto/datapoint.pb.h>

namespace sensateiot::util
{
	template <>
	std::string to_protobuf<std::vector<models::RawMeasurement>>(const std::vector<models::RawMeasurement>& value)
	{
		MeasurementData data;

		for(const auto& entry : value) {
			auto* measurement = data.add_measurements();

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
		}

		std::vector<std::uint8_t> bytes(data.ByteSizeLong());
		data.SerializeToArray(bytes.data(), bytes.size());

		return Encode64(bytes);
	}
}
