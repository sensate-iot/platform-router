/*
 * Validation test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>
#include <iostream>
#include <re2/re2.h>
#include <cassert>

#include <sensateiot/data/measurementvalidator.h>
#include <sensateiot/data/bulkmeasurementvalidator.h>
#include <sensateiot/util/sha256.h>

#include <string_view>
#include <iostream>
#include <fstream>

static constexpr std::string_view json(R"({"longitude":4.774186840897145,"latitude":51.59384817617493,"sensorId":"5c7c3bbd80e8ae3154d04912","secret":"$4ed836205f02c2d353f5620e762045003a2c512cffc0495f7671002c343183d1==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})");
static const RE2 search_regex("\\$[a-f0-9]{64}==");

static constexpr auto bad_data = {
	std::string_view(R"({{{{"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})"),
	std::string_view(R"([{"longitude:4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}}]]])"),
	std::string_view(R"({"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})"),
	std::string_view(R"({"longitude":4.774186840897145,}}{"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})"),
	std::string_view(R"({{"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}||}})")
};

static void test_bad_input()
{
	sensateiot::data::MeasurementValidator validator;
	sensateiot::data::BulkMeasurementValidator bulkValidator;
	auto count = 0;
	std::cout << "Testing bad input..." << std::endl;

	for (auto& inputView : bad_data) {
		std::string input(inputView);

		try {
			auto result = validator(input);

			if (result.first) {
				count += 1;
			}
		} catch (std::exception&) {
			count += 1;
		}

		try {
			auto result = bulkValidator(input);

			if (result.has_value()) {
				count += 1;
			}
		} catch (std::exception&) {
			count += 1;
		}
	}

	if (count > 0) {
		throw std::exception();
	}

	std::cerr << "Done testing bad input: " << count << " error occurred!";
}

static void validate_bulk()
{
	std::stringstream sstream;
	sensateiot::data::BulkMeasurementValidator bmv;
	using ClockType = std::chrono::high_resolution_clock;

	sstream << '[';
	for (auto idx = 0UL; idx < 99; idx++) {
		sstream << json << ',';
	}

	sstream << json;
	sstream << ']';

	std::cout << "Parsing measurements..." << std::endl;
	std::string str = sstream.str();

	ClockType::time_point start = ClockType::now();

	auto result = bmv(str);
	ClockType::time_point end = ClockType::now();

	std::cout << "Parsed " << result.value().size() << " measurements." << std::endl;
	auto diff = end - start;
	std::cout << "Parsing took: " << std::chrono::duration_cast<std::chrono::microseconds>(diff).count() << "us." << std::endl;
}

static void validate_indiv()
{
	sensateiot::data::MeasurementValidator validator;
	std::vector<sensateiot::models::Measurement> data;
	using ClockType = std::chrono::high_resolution_clock;

	std::cout << "Validating individual messages: " << std::endl;
	std::cout << "Parsing measurements..." << std::endl;
	ClockType::time_point start = ClockType::now();

	for (auto idx = 0UL; idx < 100; idx++) {
		auto result = validator(std::string(json));

		if (!result.first) {
			throw std::exception();
			continue;
		}

		data.emplace_back(std::move(result.second));
	}

	ClockType::time_point end = ClockType::now();
	std::cout << "Parsed " << data.size() << " measurements." << std::endl;
	auto diff = end - start;
	std::cout << "Parsing took: " << std::chrono::duration_cast<std::chrono::microseconds>(diff).count() << "us." << std::endl;
}

static void authorize_message()
{
	std::string message(json);

	assert(search_regex.ok());

	auto result = RE2::Replace(&message, search_regex, "Hello, World!");
	(void)result;
	assert(result);

	auto compare = sensateiot::util::sha256_compare(message, "4ed836205f02c2d353f5620e762045003a2c512cffc0495f7671002c343183d1");

	if(!compare) {
		std::cout << "Invalid hash compare!" << std::endl;
		abort();
	}
}

int main(int argc, char** argv)
{
	try {
		authorize_message();
		validate_bulk();
		validate_indiv();
		test_bad_input();
	} catch (std::exception&) {
		std::cerr << "Unable to complete validator test!" << std::endl;
		std::exit(1);
	}

	return -EXIT_SUCCESS;
}
