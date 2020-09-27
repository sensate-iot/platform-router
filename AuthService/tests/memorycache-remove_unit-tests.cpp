/*
 * Memory cache remove unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <thread>

#include <catch2/catch.hpp>
#include <sensateiot/data/memorycache.h>

using namespace sensateiot::data;

TEST_CASE("Can remove item", "[remove]")
{
	MemoryCache<int, int> cache;

	cache.Add({ 1, 1 }, {});
	cache.Remove(1);

	REQUIRE(cache.Count() == 0);
}

TEST_CASE("Can remove expired item", "[remove, expire]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));

	cache.Add({ 1, 1 }, {});
	REQUIRE(cache.Count() == 1);

	std::this_thread::sleep_for(std::chrono::milliseconds(60));

	auto result = cache.TryGetValue(1);
	REQUIRE_FALSE(result.has_value());

	cache.Remove(1);
	REQUIRE(cache.Count() == 0);
}

TEST_CASE("Cannot remove non-existing item", "[remove]")
{
	MemoryCache<int, int> cache;

	REQUIRE_THROWS_AS(cache.Remove(1), std::out_of_range);
	REQUIRE(cache.Count() == 0);
}
