/*
 * Unit tests for the Sensate IoT memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <thread>

#include <catch2/catch.hpp>
#include <sensateiot/cache/memorycache.h>

using namespace sensateiot::cache;

TEST_CASE("Can succesfully perform a lookup", "[lookup]")
{
	MemoryCache<sensateiot::models::ObjectId, int> cache(500);
	MemoryCacheEntryOptions options;
	sensateiot::models::ObjectId id("5f67585f036e36a6a4a3bcfb");

	options.SetSize(10);
	cache.Add({ id, 10 }, options);
	auto result = cache.TryGetValue(id);

	REQUIRE(result.has_value());
	REQUIRE(result.value() == 10);
}

TEST_CASE("Can succesfully perform a (const) bracket lookup", "[lookup]")
{
	MemoryCache<sensateiot::models::ObjectId, int> cache;
	sensateiot::models::ObjectId id("5f67585f036e36a6a4a3bcfb");

	cache.Add({ id, 10 }, {});
	const auto readback = cache[id];

	REQUIRE(readback == 10);
}

TEST_CASE("Const bracket operator throws when key is not found", "[lookup]")
{
	MemoryCache<sensateiot::models::ObjectId, int> cache;
	sensateiot::models::ObjectId id("5f67585f036e36a6a4a3bcfb");

	REQUIRE_THROWS_AS(std::as_const(cache)[id], std::out_of_range);
}

TEST_CASE("Can succesfully perform a bracket lookup", "[lookup]")
{
	MemoryCache<sensateiot::models::ObjectId, int> cache;
	sensateiot::models::ObjectId id("5f67585f036e36a6a4a3bcfb");

	cache.Add({ id, 10 }, {});
	const auto readback = cache[id];

	REQUIRE(readback == 10);
	REQUIRE(cache.Count() == 1);
}

TEST_CASE("Bracket operator does not throw when key is not found", "[lookup]")
{
	MemoryCache<sensateiot::models::ObjectId, int> cache;
	sensateiot::models::ObjectId id("5f67585f036e36a6a4a3bcfb");

	auto result = cache[id];
	REQUIRE(result == int());
	REQUIRE(cache.Count() == 1);
}

TEST_CASE("Cannot find non-existent items", "[lookup]")
{
	MemoryCache<int, int> cache(500);
	MemoryCacheEntryOptions options;

	options.SetSize(10);
	cache.Add({ 1, 10 }, options);
	auto result = cache.TryGetValue(2);

	REQUIRE_FALSE(result.has_value());
}

TEST_CASE("Entry does timeout", "[lookup, timeout]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));

	cache.Add({ 1, 1 }, {});
	cache.Add({ 2, 2 }, {});
	cache.Add({ 3, 3 }, {});

	auto r1 = cache.TryGetValue(1);

	REQUIRE(r1.has_value());
	REQUIRE(r1.value() == 1);
	REQUIRE(cache.Count() == 3);

	std::this_thread::sleep_for(std::chrono::milliseconds(51));
	auto r2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 3);
	REQUIRE_FALSE(r2.has_value());
}

TEST_CASE("Can check if a timed-out key exists", "[contains]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));

	cache.Add({ 1, 2 }, {});
	REQUIRE(cache.Contains(1));
	REQUIRE_FALSE(cache.Contains(2));
	
	std::this_thread::sleep_for(std::chrono::milliseconds(55));
	
	REQUIRE_FALSE(cache.Contains(1));
	REQUIRE_FALSE(cache.Contains(2));
}
