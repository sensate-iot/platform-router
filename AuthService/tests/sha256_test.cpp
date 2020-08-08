/*
 * SHA256 hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <iostream>

#include <boost/algorithm/hex.hpp>

#include <sensateiot/util/sha256.h>

int main(int argc, char** argv)
{
	SHA256_CTX ctx;
	std::string txt1 = "Hello, World!";
	std::uint8_t result[SHA256_BLOCK_SIZE];

	sha256_init(&ctx);
	sha256_update(&ctx, reinterpret_cast<const uint8_t *>(txt1.c_str()), txt1.length());
	sha256_final(&ctx, result);

	std::vector<std::uint8_t> input(SHA256_BLOCK_SIZE);
	input.assign(result, result + SHA256_BLOCK_SIZE);
	std::string output;

	boost::algorithm::hex_lower(input.begin(), input.end(), std::back_inserter(output));

	if(output != "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f") {
		std::cerr << "SHA256 hash invalid!" << std::endl;
		return -EXIT_FAILURE;
	}
	
	return -EXIT_SUCCESS;
}
