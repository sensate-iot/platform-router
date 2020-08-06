/*
 * GZIP hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <boost/algorithm/hex.hpp>

#include <sensateiot/util/gzip.h>
#include <sensateiot/util/base64.h>

int main(int argc, char** argv)
{
	std::vector<char> data{'a', 'b', 'c', 'a', 'a'};
	std::vector<char> raw{'a', 'b', 'c', 'a', 'a'};
	auto compressed = sensateiot::util::Compress(data);
	auto b64 = sensateiot::util::Encode64(raw);

	if(compressed != "H4sIAAAAAAAA/0tMSk5MBAA56J3/BQAAAA==") {
		
	}

	if(b64 != "YWJjYWE=") {
		
	}
	
	return -EXIT_SUCCESS;
}
