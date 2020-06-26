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
#include <sensateiot/util/sha256.h>
#include <string_view>

//static constexpr std::string_view json("{\"longitude\":4.774186840897145,\"latitude\":51.59384817617493,\"createdById\":\"5c7c3bbd80e8ae3154d04912\",\"createdBySecret\":\"$5e7a36d90554c9b805345533de22eafbb55b081c69fed55c8311f46b0e45527b==\",\"data\":{\"x\":{\"value\":3.7348298850142325,\"unit\":\"m/s2\"},\"y\":{\"value\":95.1696675190223,\"unit\":\"m/s2\"},\"z\":{\"value\":15.24488164994629,\"unit\":\"m/s2\"}}}");
static constexpr std::string_view json(R"({"longitude":4.774186840897145,"latitude":51.59384817617493,"createdById":"5c7c3bbd80e8ae3154d04912","createdBySecret":"$76d0d71b0abb9681a5984de91d07b7f434424492933d3069efa2a18e325bd911==","data":{"x":{"value":3.7348298850142325,"unit":"m/s2"},"y":{"value":95.1696675190223,"unit":"m/s2"},"z":{"value":15.24488164994629,"unit":"m/s2"}}})");
//static constexpr auto search_regex = ctll::fixed_string{ "\\$[a-f0-9]{64}==" };
static const RE2 search_regex("\\$[a-f0-9]{64}==");

static void authorize_message()
{
	std::string message(json);

	assert(search_regex.ok());

	auto result = RE2::Replace(&message, search_regex, "Hello, World!");
	assert(result);

	std::cout << message << std::endl;

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
	return -EXIT_SUCCESS;
}
