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

TEST_CASE("Can update an item using AddOrUpdate", "[update]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 2 }, {});
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r1.has_value());
	REQUIRE(*r1 == 2);

	cache.AddOrUpdate({ 1, 3 }, {});
	
	auto r2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r2.has_value());
	REQUIRE(*r2 == 3);
}

TEST_CASE("Can update (overwrite) expired key", "[add, expired]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));
	
	cache.Add({ 1, 2 }, {});
	auto rb1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb1.has_value());
	REQUIRE(*rb1 == 2);

	std::this_thread::sleep_for(std::chrono::milliseconds(60));
	cache.Update({ 1, 4 }, {});
	auto rb2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb2.has_value());
	REQUIRE(*rb2 == 4);
}

TEST_CASE("Can update (overwrite) expired key (bracket operator)", "[add, expired]")
{
	MemoryCache<int, int> cache(std::chrono::milliseconds(50));
	
	cache.Add({ 1, 2 }, {});
	auto rb1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb1.has_value());
	REQUIRE(*rb1 == 2);

	std::this_thread::sleep_for(std::chrono::milliseconds(60));
	cache[1] = 4;
	auto rb2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(rb2.has_value());
	REQUIRE(*rb2 == 4);
}

TEST_CASE("Cannot update non-existing entry", "[update]")
{
	MemoryCache<int, int> cache;

	REQUIRE_THROWS_AS(cache.Update({ 1, 1 }, { }), std::out_of_range);
	REQUIRE(cache.Count() == 0);
	REQUIRE(cache.Size() == 0);
	REQUIRE(cache.Capacity() == 0);
}

TEST_CASE("Can update an item using Update", "[update]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 2 }, {});
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r1.has_value());
	REQUIRE(*r1 == 2);

	cache.Update({ 1, 3 }, {});
	
	auto r2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r2.has_value());
	REQUIRE(*r2 == 3);
}

TEST_CASE("Can update multiple items", "[update, range]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 1 }, {});
	cache.Add({ 2, 2 }, {});
	cache.Add({ 3, 3 }, {});
	
	REQUIRE(cache.Count() == 3);
	REQUIRE(cache.Capacity() == 0);
	REQUIRE(cache.Size() == 0);

	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	cache.UpdateRange(entries, {});
	
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

	REQUIRE(cache.Capacity() == 0);
	REQUIRE(cache.Size() == 0);
}

TEST_CASE("Can move update", "[update, move]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 2 }, {});
	auto entry = std::make_pair(1, 3);

	cache.Update(std::move(entry), { });
	REQUIRE(cache.Count() == 1);
	REQUIRE(cache.Size() == 0);
	REQUIRE(cache.Capacity() == 0);
	
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(r1.has_value());
	REQUIRE(r1.value() == 3);
}

TEST_CASE("Cannot move update non-existent item", "[update, move]")
{
	MemoryCache<int, int> cache;
	auto entry = std::make_pair(1, 1);

	REQUIRE_THROWS_AS(cache.Update(std::move(entry), { }), std::out_of_range);
	REQUIRE(cache.Count() == 0);
	REQUIRE(cache.Size() == 0);
	REQUIRE(cache.Capacity() == 0);
}

TEST_CASE("Can move update expired item", "[update, move]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 2 }, {});
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r1.has_value());
	REQUIRE(*r1 == 2);
	
	auto entry = std::make_pair(1, 3);
	cache.Update(std::move(entry), { });
	auto r2 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 1);
	REQUIRE(r2.has_value());
	REQUIRE(*r2 == 3);

}

TEST_CASE("Cannot overflow using range update", "[update, range]")
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

	options.SetSize(2);
	REQUIRE_THROWS_AS(cache.UpdateRange(entries, options), std::overflow_error);
	
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

TEST_CASE("Cannot overflow using update", "[update]")
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

	options.SetSize(2);
	REQUIRE_THROWS_AS(cache.Update({ 1,2 }, options), std::overflow_error);
	
	auto r1 = cache.TryGetValue(1);
	
	REQUIRE(cache.Count() == 3);
	REQUIRE(cache.Size() == 3);
	REQUIRE(r1.has_value());
	REQUIRE(*r1 == 1);
}

TEST_CASE("Can update a range using AddOrUpdate", "[update, range]")
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

	cache.AddOrUpdateRange(entries, options);
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 4);
	REQUIRE(*r2 == 5);
	REQUIRE(*r3 == 6);

	REQUIRE(cache.Count() == 3);
	REQUIRE(cache.Capacity() == 3);
	REQUIRE(cache.Size() == 3);
}

TEST_CASE("Can update a range using AddOrUpdate with rvalues", "[update, range, rvalues]")
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

	cache.AddOrUpdateRange(std::move(entries), options);
	
	auto r1 = cache.TryGetValue(1);
	auto r2 = cache.TryGetValue(2);
	auto r3 = cache.TryGetValue(3);
	
	REQUIRE(r1.has_value());
	REQUIRE(r2.has_value());
	REQUIRE(r3.has_value());
	
	REQUIRE(*r1 == 4);
	REQUIRE(*r2 == 5);
	REQUIRE(*r3 == 6);

	REQUIRE(cache.Count() == 3);
	REQUIRE(cache.Capacity() == 3);
	REQUIRE(cache.Size() == 3);
}

TEST_CASE("Can update a range using Update with rvalues", "[update, range, rvalues]")
{
	MemoryCache<int, int> cache;
	
	cache.Add({ 1, 1 }, {});
	cache.Add({ 2, 2 }, {});
	cache.Add({ 3, 3 }, {});
	
	REQUIRE(cache.Count() == 3);
	REQUIRE(cache.Capacity() == 0);
	REQUIRE(cache.Size() == 0);

	std::vector<MemoryCache<int, int>::EntryType> entries;

	entries.push_back({ 1, 4 });
	entries.push_back({ 2, 5 });
	entries.push_back({ 3, 6 });

	cache.UpdateRange(entries, {});
	
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

	REQUIRE(cache.Capacity() == 0);
	REQUIRE(cache.Size() == 0);
}
