/*
 * Unit tests for the Sensate IoT memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <thread>

#include <catch2/catch.hpp>
#include <sensateiot/data/memorycache.h>

using namespace sensateiot::data;

TEST_CASE("Can add item", "[add]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 2 }, {});
	auto readback = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(readback.has_value());
	REQUIRE(*readback == 2);
}

TEST_CASE("Can add (overwrite) expired key", "[add, expired]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));
	
	cache.Add({ 1, 2 }, {});
	auto rb1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb1.has_value());
	REQUIRE(*rb1 == 2);

	std::this_thread::sleep_for(std::chrono::milliseconds(60));
	cache.Add({ 1, 4 }, {});
	auto rb2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb2.has_value());
	REQUIRE(*rb2 == 4);
}

TEST_CASE("Cannot add existing key", "[add]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 2 }, {});
	REQUIRE_THROWS_AS(cache.Add({ 1,1 }, {}), std::out_of_range);
}

TEST_CASE("Can add entry using bracket operator", "[add, bracket]")
{
	MemoryCache<int, int> cache;

	cache[5] = 1;

	REQUIRE(cache.Count() == 1);
}

TEST_CASE("Can add an item using AddOrUpdate", "[update]")
{
	MemoryCache<int, int> cache;
	
	cache.AddOrUpdate({ 1, 2 }, {});
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r1.has_value());
	REQUIRE(*r1 == 2);
}

TEST_CASE("Cannot overflow using a range add", "[add, overflow]")
{
	MemoryCache<int, int> cache(3);
	MemoryCacheEntryOptions options;

	options.SetSize(1);
	
	cache.Add({ 1, 1 }, options);
	cache.Add({ 2, 2 }, options);
	cache.Add({ 3, 3 }, options);
	
	REQUIRE(cache.Count() == 3);
	REQUIRE(cache.Capacity() == 3);
	REQUIRE(cache.Size() == 3);

	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	REQUIRE_THROWS_AS(cache.AddRange(entries, options), std::out_of_range);
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(cache.Count() == 3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 1);
	REQUIRE(*r2 == 2);
	REQUIRE(*r3 == 3);

	REQUIRE(cache.Capacity() == 3);
	REQUIRE(cache.Size() == 3);
}

TEST_CASE("Cannot overflow", "[add, overflow]")
{
	MemoryCache<int, int> cache(10);
	MemoryCacheEntryOptions options;

	options.SetSize(5);

	cache.Add({ 1, 1 }, options);
	cache.Add({ 2, 1 }, options);
	REQUIRE_THROWS_AS(cache.Add({ 3, 1 }, options), std::overflow_error);
	REQUIRE(cache.Count() == 2);
	REQUIRE(cache.Size() == 10);
}

TEST_CASE("Can add multiple items", "[update, range]")
{
	MemoryCache<int, int> cache;
	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	cache.AddRange(entries, {});
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(cache.Count() == 3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 4);
	REQUIRE(*r2 == 5);
	REQUIRE(*r3 == 6);
}

TEST_CASE("Can add multiple items using rvalues", "[update, range, rvalues]")
{
	MemoryCache<int, int> cache;
	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	cache.AddRange(std::move(entries), {});
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(cache.Count() == 3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 4);
	REQUIRE(*r2 == 5);
	REQUIRE(*r3 == 6);

}

TEST_CASE("Can move add", "[add, move]")
{
	MemoryCache<int, int> cache;
	
	auto entry = std::make_pair(1, 3);
	cache.Add(std::move(entry), { });
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(cache.Size() == 0);
	REQUIRE(cache.Capacity() == 0);
	REQUIRE(r1.has_value());
	REQUIRE(r1.value() == 3);
}

TEST_CASE("Can move add (overwrite) expired key", "[add, expired]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));
	auto e1 = std::make_pair(1, 2);
	auto e2 = std::make_pair(1, 4);
	
	cache.Add(std::move(e1), {});
	auto rb1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb1.has_value());
	REQUIRE(*rb1 == 2);

	std::this_thread::sleep_for(std::chrono::milliseconds(60));
	cache.Add(std::move(e2), {});
	auto rb2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb2.has_value());
	REQUIRE(*rb2 == 4);
}

TEST_CASE("Can add a range using AddOrUpdateRange", "[add]")
{
	MemoryCache<int, int> cache;
	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	cache.AddOrUpdateRange(entries, {});
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(cache.Count() == 3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 4);
	REQUIRE(*r2 == 5);
	REQUIRE(*r3 == 6);
}

TEST_CASE("Can add a range using AddOrUpdate with rvalues", "[add, range, rvalues]")
{
	MemoryCache<int, int> cache;
	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	cache.AddOrUpdateRange(std::move(entries), {});
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(cache.Count() == 3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 4);
	REQUIRE(*r2 == 5);
	REQUIRE(*r3 == 6);
}
