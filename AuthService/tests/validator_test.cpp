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

static constexpr std::string_view json(R"({"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})");
static const RE2 search_regex("\\$[a-f0-9]{64}==");

static void validate_bulk()
{
	std::stringstream sstream;
	sensateiot::data::BulkMeasurementValidator bmv;

	sstream << '[';
	for (auto idx = 0UL; idx < 99; idx++) {
		sstream << json << ',';
	}

	sstream << json;

	sstream << ']';

	using ClockType = std::chrono::high_resolution_clock;

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

	std::cout << "Parsing measurements..." << std::endl;
	ClockType::time_point start = ClockType::now();

	for (auto idx = 0UL; idx < 100; idx++) {
		auto result = validator(std::string(json));

		if (!result.first) {
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

	auto compare = sensateiot::util::sha256_compare(message, "76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911");

	if(!compare) {
		std::cout << "Invalid hash compare!" << std::endl;
		abort();
	}
}

int main(int argc, char** argv)
{
	sensateiot::data::MeasurementValidator validator;

	validator(std::string(json));
	authorize_message();
	validate_bulk();

	std::cout << "Validating individual messages: " << std::endl;
	validate_indiv();
	return -EXIT_SUCCESS;
}
