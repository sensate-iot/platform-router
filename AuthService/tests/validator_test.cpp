/*
 * Validation test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>

#include <sensateiot/data/measurementvalidator.h>

static constexpr std::string_view json("{\"longitude\":4.774186840897145,\"latitude\":51.59384817617493,\"createdById\":\"5c7c3bbd80e8ae3154d04912\",\"createdBySecret\":\"$5e7a36d90554c9b805345533de22eafbb55b081c69fed55c8311f46b0e45527b==\",\"data\":{\"x\":{\"value\":3.7348298850142325,\"unit\":\"m/s2\"},\"y\":{\"value\":95.1696675190223,\"unit\":\"m/s2\"},\"z\":{\"value\":15.24488164994629,\"unit\":\"m/s2\"}}}");

int main(int argc, char** argv)
{
	sensateiot::data::MeasurementValidator validator;

	validator(std::string(json));
	return -EXIT_SUCCESS;
}
