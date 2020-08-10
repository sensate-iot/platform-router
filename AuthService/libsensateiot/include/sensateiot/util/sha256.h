/*
 * SHA256 hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <stdint.h>

#define SHA256_BLOCK_SIZE 32

#ifdef __cplusplus

#include <sensateiot.h>
#include <array>
#include <string>
#include <sensateiot/stl/smallvector.h>

namespace sensateiot::util
{
	static constexpr std::size_t Sha256Size = SHA256_BLOCK_SIZE;
	extern DLL_EXPORT stl::SmallVector<std::uint8_t, Sha256Size> sha256(const std::string& data);
	extern DLL_EXPORT bool sha256_compare(const std::string& data, const std::string& other);
}

extern "C" {
#endif

typedef struct {
	uint8_t data[64];
	uint32_t datalen;
	unsigned long long bitlen;
	uint32_t state[8];
} SHA256_CTX;

extern void sha256_init(SHA256_CTX *ctx);
extern void sha256_update(SHA256_CTX *ctx, const uint8_t* data, size_t len);
extern void sha256_final(SHA256_CTX *ctx, uint8_t* hash);

#ifdef __cplusplus
}
#endif
