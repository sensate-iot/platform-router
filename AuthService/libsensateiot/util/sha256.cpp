/*
 * SHA256 hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/util/sha256.h>
#include <boost/algorithm/hex.hpp>
#include <sensateiot/stl/smallvector.h>

namespace sensateiot::util
{
	stl::SmallVector<std::uint8_t, Sha256Size> sha256(const std::string& data)
	{
		std::uint8_t values[Sha256Size];
		stl::SmallVector<std::uint8_t, Sha256Size> rv{};
		SHA256_CTX ctx;

		sha256_init(&ctx);
		sha256_update(&ctx, reinterpret_cast<const uint8_t *>(data.c_str()), data.length());
		sha256_final(&ctx, values);
		std::move(std::begin(rv), std::end(rv), std::back_inserter(rv));

		return rv;
	}

	bool sha256_compare(const std::string& data, const std::string& other)
	{
		stl::SmallVector<std::uint8_t, Sha256Size> otherHex;
		std::uint8_t values[Sha256Size];

		boost::algorithm::unhex(other.begin(), other.end(), std::back_inserter(otherHex));
		auto result = sha256(data);
		SHA256_CTX ctx;

		sha256_init(&ctx);
		sha256_update(&ctx, reinterpret_cast<const uint8_t *>(data.c_str()), data.length());
		sha256_final(&ctx, values);

		return memcmp(otherHex.data(), values, Sha256Size) == 0;
	}
}
