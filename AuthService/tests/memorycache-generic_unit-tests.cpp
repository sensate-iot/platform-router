/*
 * Unit tests for the Sensate IoT memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <thread>

#include <catch2/catch.hpp>
#include <sensateiot/data/memorycache.h>

using namespace sensateiot::data;

TEST_CASE("Cache can be constructed", "[construct, destruct]")
{
	MemoryCache<int, int> defaultConstructed;
	MemoryCache<int, int> capacityOnly(500);
	MemoryCache<int, int> timeoutOnly(std::chrono::milliseconds(500));
	MemoryCache<int, int> cache(std::chrono::milliseconds(500), 500);

	REQUIRE(capacityOnly.Count() == 0);
	REQUIRE(defaultConstructed.Count() == 0);
	REQUIRE(timeoutOnly.Count() == 0);
	REQUIRE(cache.Count() == 0);
	
	REQUIRE(capacityOnly.Capacity() == 500);
	REQUIRE(cache.Capacity() == 500);
	REQUIRE(defaultConstructed.Capacity() == 0);
	REQUIRE(timeoutOnly.Capacity() == 0);
	
	REQUIRE(timeoutOnly.Size() == 0);
	REQUIRE(defaultConstructed.Size() == 0);
	REQUIRE(capacityOnly.Size() == 0);
	REQUIRE(cache.Size() == 0);
}

TEST_CASE("Cache can copy construct", "[construction]")
{
	MemoryCache<int, int> defaultConstructed(std::chrono::minutes(5), 500);
	MemoryCacheEntryOptions options;

	options.SetSize(2);

	defaultConstructed.Add({ 1, 1 }, options);
	defaultConstructed.Add({ 2, 2 }, options);
	defaultConstructed.Add({ 3, 3 }, options);

	MemoryCache<int, int> copyConstructed(defaultConstructed);

	REQUIRE(copyConstructed.Capacity() == 500);
	REQUIRE(copyConstructed.Size() == 6);
	REQUIRE(copyConstructed.Count() == 3);
	REQUIRE(copyConstructed.Timeout() == std::chrono::minutes(5));

	auto r1 = copyConstructed.TryGetValue(1);
	auto r2 = copyConstructed.TryGetValue(2);
	auto r3 = copyConstructed.TryGetValue(3);

	REQUIRE(r1.value() == 1);
	REQUIRE(r2.value() == 2);
	REQUIRE(r3.value() == 3);
}

TEST_CASE("Cache can move construct", "[construction]")
{
	
	MemoryCache<int, int> defaultConstructed(std::chrono::minutes(5), 500);
	MemoryCacheEntryOptions options;

	options.SetSize(2);

	defaultConstructed.Add({ 1, 1 }, options);
	defaultConstructed.Add({ 2, 2 }, options);
	defaultConstructed.Add({ 3, 3 }, options);

	MemoryCache<int, int> moveConstructed(std::move(defaultConstructed));

	REQUIRE(moveConstructed.Capacity() == 500);
	REQUIRE(moveConstructed.Size() == 6);
	REQUIRE(moveConstructed.Count() == 3);
	REQUIRE(moveConstructed.Timeout() == std::chrono::minutes(5));

	auto r1 = moveConstructed.TryGetValue(1);
	auto r2 = moveConstructed.TryGetValue(2);
	auto r3 = moveConstructed.TryGetValue(3);

	REQUIRE(r1.value() == 1);
	REQUIRE(r2.value() == 2);
	REQUIRE(r3.value() == 3);
}

TEST_CASE("Cache can be copy assigned", "[construction]")
{
	MemoryCache<int, int> defaultConstructed(std::chrono::minutes(5), 500);
	MemoryCache<int, int> copyAssigned;
	MemoryCacheEntryOptions options;

	options.SetSize(2);

	defaultConstructed.Add({ 1, 1 }, options);
	defaultConstructed.Add({ 2, 2 }, options);
	defaultConstructed.Add({ 3, 3 }, options);

	copyAssigned = defaultConstructed;

	REQUIRE(copyAssigned.Capacity() == 500);
	REQUIRE(copyAssigned.Size() == 6);
	REQUIRE(copyAssigned.Count() == 3);
	REQUIRE(copyAssigned.Timeout() == std::chrono::minutes(5));

	auto r1 = copyAssigned.TryGetValue(1);
	auto r2 = copyAssigned.TryGetValue(2);
	auto r3 = copyAssigned.TryGetValue(3);

	REQUIRE(r1.value() == 1);
	REQUIRE(r2.value() == 2);
	REQUIRE(r3.value() == 3);
}

TEST_CASE("Cache can be move assigned", "[construction]")
{
	MemoryCache<int, int> defaultConstructed(std::chrono::minutes(5), 500);
	MemoryCache< int, int > moveAssigned;
	MemoryCacheEntryOptions options;

	options.SetSize(2);

	defaultConstructed.Add({ 1, 1 }, options);
	defaultConstructed.Add({ 2, 2 }, options);
	defaultConstructed.Add({ 3, 3 }, options);

	moveAssigned = std::move(defaultConstructed);

	REQUIRE(moveAssigned.Capacity() == 500);
	REQUIRE(moveAssigned.Size() == 6);
	REQUIRE(moveAssigned.Count() == 3);
	REQUIRE(moveAssigned.Timeout() == std::chrono::minutes(5));

	auto r1 = moveAssigned.TryGetValue(1);
	auto r2 = moveAssigned.TryGetValue(2);
	auto r3 = moveAssigned.TryGetValue(3);

	REQUIRE(r1.value() == 1);
	REQUIRE(r2.value() == 2);
	REQUIRE(r3.value() == 3);
}

TEST_CASE("Can be cleared", "[cleanup]")
{
	MemoryCache<int, int> defaultConstructed;

	defaultConstructed.Add({ 1, 1 }, {});
	defaultConstructed.Add({ 2, 1 }, {});
	defaultConstructed.Add({ 3, 1 }, {});

	defaultConstructed.Clear();
	auto r1 = defaultConstructed.TryGetValue(1);
	auto r2 = defaultConstructed.TryGetValue(2);
	auto r3 = defaultConstructed.TryGetValue(3);

	REQUIRE_FALSE(r1.has_value());
	REQUIRE_FALSE(r2.has_value());
	REQUIRE_FALSE(r3.has_value());

	REQUIRE(defaultConstructed.Count() == 0);
}

TEST_CASE("Cache can cleanup", "[cleanup, timeout]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(500));
	MemoryCacheEntryOptions options;

	options.SetTimeout(std::chrono::seconds(10));

	cache.Add({ 1, 1 }, options);
	cache.Add({ 2, 2 }, {});
	cache.Add({ 3, 3 }, {});
	
	REQUIRE(cache.Count() == 3);
	
	std::this_thread::sleep_for(std::chrono::seconds(1));
	
	REQUIRE(cache.Count() == 3);
	
	auto result = cache.Cleanup(std::chrono::milliseconds(200));
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(result);
}

TEST_CASE("Can change capacity", "[capacity]")
{
	MemoryCache<int, int> defaultCache;
	MemoryCache<int, int> cappedCache(500);

	REQUIRE_THROWS_AS(defaultCache.SetCapacity(500), std::runtime_error);
	cappedCache.SetCapacity(750);

	REQUIRE(cappedCache.Capacity() == 750);
	REQUIRE(defaultCache.Capacity() == 0);
}

TEST_CASE("Count increases on add", "[cleanup]")
{
	MemoryCache<int, int> cache;

	REQUIRE(cache.Count() == 0);
	cache.Add({ 1,1 }, {});
	REQUIRE(cache.Count() == 1);
}

TEST_CASE("Count decreases on remove", "[cleanup]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1,1 }, {});
	REQUIRE(cache.Count() == 1);
	cache.Remove(1);
	REQUIRE(cache.Count() == 0);
}

TEST_CASE("Size increases on add", "[cleanup]")
{
	MemoryCache<int, int> cache(500);
	MemoryCacheEntryOptions options;

	options.SetSize(10);
	REQUIRE(cache.Size() == 0);
	cache.Add({ 1,1 }, options);
	REQUIRE(cache.Size() == 10);
}

TEST_CASE("Size decreases on remove", "[cleanup]")
{
	MemoryCache<int, int> cache(500);
	MemoryCacheEntryOptions options;

	options.SetSize(10);
	cache.Add({ 1,1 }, options);
	REQUIRE(cache.Size() == 10);
	cache.Remove(1);
	REQUIRE(cache.Size() == 0);
}
