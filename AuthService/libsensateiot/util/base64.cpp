/*
 * Base64 header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <vector>
#include <string>
#include <stdexcept>
#include <cstdlib>
#include <limits>
#include <cstring>

#include <openssl/evp.h>

#include <sensateiot/util/base64.h>

namespace sensateiot::util {
	std::string Encode64(const std::vector<char>& input)
	{
		std::size_t length = 4 * ((input.size() + 2) / 3);;
		auto* str = new unsigned char[length + 1];

		memset(str, 0, length + 1);
		if(input.size() > std::numeric_limits<int>::max()) {
			throw std::range_error("Unable to Encode64: input too big!");
		}

		EVP_EncodeBlock(str, reinterpret_cast<const std::uint8_t*>(input.data()), static_cast<int>(input.size()));

		auto result = std::string((char*)str);
		delete[] str;
		return result;
	}
}