/*
 * GZIP hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <iostream>

#include <boost/algorithm/hex.hpp>

#include <sensateiot/util/gzip.h>
#include <sensateiot/util/base64.h>

static void test_zip()
{
	std::vector<char> data{'a', 'b', 'c', 'a', 'a'};
	auto compressed = sensateiot::util::Compress(data);

	if(compressed != "H4sIAAAAAAAA/0tMSk5MBAA56J3/BQAAAA==") {
		throw std::exception();
	}
}

static void test_base64_encode()
{
	std::vector<char> raw{'a', 'b', 'c', 'a', 'a'};
	auto b64 = sensateiot::util::Encode64(raw);

	if(b64 != "YWJjYWE=") {
		throw std::exception();
	}
}

int main(int argc, char** argv)
{
	try {
		test_zip();
		test_base64_encode();
	} catch (std::exception&) {
		std::cerr << "Unable to complete gzip test!" << std::endl;
		std::exit(1);
	}
	
	return -EXIT_SUCCESS;
}
