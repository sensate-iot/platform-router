/*
 * Map/red-black tree test.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <cstdlib>
#include <chrono>
#include <vector>
#include <thread>
#include <iostream>

#include <mongoc.h>

#include <sensateiot/models/sensor.h>
#include <boost/uuid/random_generator.hpp>

#include <sensateiot/util/log.h>
#include <sensateiot/cache/memorycache.h>

//static constexpr int MapSize     = 15000000;
//static constexpr int LookupSize  = 1000000;
//static constexpr int DeleteSize  = 50000;
//static constexpr int DeleteStart = 150000;

static constexpr int MapSize = 1000000;
static constexpr int DeleteSize = MapSize / 2;
static constexpr int DeleteStart = 0;
static constexpr int LookupSize = 500000;

static std::vector<boost::uuids::uuid> sensorOwnerIds;
static std::vector<sensateiot::models::ObjectId> sensorIds;
static std::vector<std::string> sensorStringIds;

static void timeout_test()
{
	using namespace sensateiot::cache;
	using TreeType = MemoryCache<std::string, int>;

	TreeType t;
	TreeType t2;

	t.Add({ std::to_string(1), 1000 }, {});
	t.Add({ std::to_string(2), 2000 }, {});
	t.Add({ std::to_string(3), 3000 }, {});
	t.Add({ std::to_string(4), 4000 }, {});

	auto v2 = t.TryGetValue(std::to_string(2));
	auto v1 = t.TryGetValue(std::to_string(1));
	auto v3 = t.TryGetValue(std::to_string(3));

	if(v1 != 1000 || v2 != 2000 || v3 != 3000) {
		throw std::exception();
	}

	using namespace std::chrono_literals;
	std::this_thread::sleep_for(2s);
	auto& log = sensateiot::util::Log::GetLog();

	t["4"] = 5000;

	if(t["4"] != 5000) {
		throw std::exception();
	}

	try {
		v2 = t.TryGetValue(std::to_string(2));
		(void)v2;
	} catch(std::out_of_range& ex) {
		(void)ex;
		log << "Correctly timed out!" << sensateiot::util::Log::NewLine;
	}

	t2 = t;
	t = t2;

	std::this_thread::sleep_for(550ms);

	t["10"] = 10;
	t.Cleanup(std::chrono::milliseconds(50));

	if(!t.Contains("10")) {
		throw std::exception();
	}
}

static void concurrent_test()
{
	static constexpr int MapSize = 10000;
	static constexpr int LookupSize = 5000;

	using namespace sensateiot::cache;
	using TreeType = MemoryCache<std::string, int>;
	TreeType t;

	auto t1 = std::thread([&t]()
	{
		static constexpr int StartInsert = 0;

		for(auto idx = StartInsert; idx < MapSize; idx++) {
			t.Add({ std::to_string(idx), idx }, {});
		}

		auto now = std::chrono::high_resolution_clock::now();

		for(auto idx = StartInsert; idx < LookupSize; idx++) {
			auto value = t.TryGetValue(std::to_string(idx), now);

			if(!value.has_value()) {
				throw std::out_of_range("Unable to complete concurrency test!");
			}
		}

		for(auto idx = StartInsert; idx < LookupSize; idx++) {
			t.Remove(std::to_string(idx));
		}
	});

	auto t2 = std::thread([&t]()
	{
		static constexpr int StartInsert = MapSize;

		for(auto idx = StartInsert; idx < MapSize * 2; idx++) {
			t.Add({ std::to_string(idx), idx }, {});
		}

		auto now = std::chrono::high_resolution_clock::now();

		for(auto idx = StartInsert; idx < LookupSize + StartInsert; idx++) {
			auto value = t.TryGetValue(std::to_string(idx), now);

			if(!value.has_value()) {
				throw std::out_of_range("Unable to complete concurrency test!");
			}
		}

		for(auto idx = StartInsert; idx < LookupSize + StartInsert; idx++) {
			t.Remove(std::to_string(idx));
		}
	});

	auto t3 = std::thread([&t]()
	{
		static constexpr int StartInsert = MapSize * 2;
		for(auto idx = StartInsert; idx < MapSize * 3; idx++) {
			t.Add({ std::to_string(idx), idx }, {});
		}

		for(auto idx = StartInsert; idx < LookupSize + StartInsert; idx++) {
			t.Remove(std::to_string(idx));
		}

		auto now = std::chrono::high_resolution_clock::now();
		
		for(auto idx = LookupSize + StartInsert; idx < MapSize * 3; idx++) {
			auto value = t.TryGetValue(std::to_string(idx), now);

			if(!value.has_value()) {
				throw std::out_of_range("Unable to complete concurrency test!");
			}
		}
	});

	auto t4 = std::thread([&t]()
	{
		static constexpr int StartInsert = MapSize * 3;
		for(auto idx = StartInsert; idx < MapSize * 4; idx++) {
			t.Add({ std::to_string(idx), idx }, {});
		}

		for(auto idx = StartInsert; idx < MapSize * 4; idx++) {
			t.Remove(std::to_string(idx));
		}
	});

	t1.join();
	t2.join();
	t3.join();
	t4.join();
}

static void test_insert()
{
	using namespace sensateiot::cache;
	using TreeType = MemoryCache<sensateiot::models::ObjectId, sensateiot::models::Sensor>;
	TreeType t;

	auto& log = sensateiot::util::Log::GetLog();

	log << "Start map test!" << sensateiot::util::Log::NewLine;
	using ClockType = std::chrono::high_resolution_clock;

	ClockType::time_point start = ClockType::now();
	using Millis = std::chrono::milliseconds;
	using Nanos = std::chrono::nanoseconds;
	MemoryCacheEntryOptions options;
	
	for(auto idx = 0UL; idx < MapSize; idx++) {
		sensateiot::models::Sensor s;
		auto id = sensorIds[idx];

		s.SetId(sensorIds[idx]);
		s.SetOwner(sensorOwnerIds[idx]);
		s.SetSecret(std::to_string(idx * 10));

		t.Add({ std::move(id), std::move(s) }, options);
	}

	ClockType::time_point end = ClockType::now();

	log << "End of map test!" << sensateiot::util::Log::NewLine;
	auto diff = end - start;
	log << "Insertion took: " << std::to_string(std::chrono::duration_cast<Millis>(diff).count())
		<< "ms." << sensateiot::util::Log::NewLine;
	log << "Insertion took: " << std::to_string(std::chrono::duration_cast<Nanos>(diff).count() / MapSize)
		<< "ns per entry." << sensateiot::util::Log::NewLine;

	log << "Start lookup-map test!" << sensateiot::util::Log::NewLine;

	using tp = std::chrono::high_resolution_clock::time_point;
	auto l = [&t]()
	{
		tp now = std::chrono::high_resolution_clock::now();

		for(auto idx = 0UL; idx < LookupSize; idx++) {
			sensateiot::models::ObjectId id(sensorStringIds[idx]);
			auto sensor = t.TryGetValue(id, now);

			if(BOOST_UNLIKELY(!sensor.has_value())) {
				throw std::out_of_range("Invalid sensor found in lookup test!");
			}
		}
	};

	start = ClockType::now();

	auto thread1 = std::thread(l);
	auto thread2 = std::thread(l);
	auto thread3 = std::thread(l);
	auto thread4 = std::thread(l);

	thread1.join();
	thread2.join();
	thread3.join();
	thread4.join();

	end = ClockType::now();

	diff = end - start;

	log << "Lookup took: " << std::to_string(std::chrono::duration_cast<Millis>(diff).count())
		<< "ms." << sensateiot::util::Log::NewLine;
	log << "Lookup took (relative): " << std::to_string(std::chrono::duration_cast<Nanos>(diff).count() / (LookupSize * 4))
		<< "ns per entry." << sensateiot::util::Log::NewLine;
	log << "Lookup took (absolute): " << std::to_string(std::chrono::duration_cast<Nanos>(diff).count() / LookupSize)
		<< "ns per entry." << sensateiot::util::Log::NewLine;

	log << "Pre-delete size: " << std::to_string(t.Count()) << sensateiot::util::Log::NewLine;
	log << "Start delete test!" << sensateiot::util::Log::NewLine;

	for(auto idx = DeleteStart; idx < (DeleteStart + DeleteSize); idx++) {
		auto sensor = t.TryGetValue(sensorIds[static_cast<unsigned long>(idx)]);

		if(!sensor.has_value()) {
			continue;
		}

		t.Remove(sensor->GetId());
	}

	log << "Post-delete size: " << std::to_string(t.Count()) << sensateiot::util::Log::NewLine;
}

int main(int argc, char** argv)
{
	using namespace sensateiot::detail;

	try {
		boost::uuids::random_generator gen;

		sensorOwnerIds.reserve(MapSize);
		sensorIds.reserve(MapSize);
		sensorStringIds.reserve(MapSize);

		for(auto idx = 0; idx < MapSize; idx++) {
			bson_oid_t oid;
			char oidStr[25];

			bson_oid_init(&oid, nullptr);
			bson_oid_to_string(&oid, oidStr);
			std::string str(oidStr);

			sensateiot::models::ObjectId id(oid.bytes);

			sensorOwnerIds.push_back(gen());
			sensorIds.push_back(id);
			sensorStringIds.emplace_back(std::move(str));
		}

		test_insert();
		concurrent_test();
		timeout_test();
	} catch(std::exception& ex) {
		std::cerr << "Unable to complete map test!" << std::endl;
		std::cerr << ex.what() << std::endl;
		std::exit(1);
	}

	return -EXIT_SUCCESS;
}



/*int main(int argc, char** argv)
{
	using namespace sensateiot::detail;
	
	try {
		MemoryCache<int, int> cache;
		MemoryCache<sensateiot::models::ObjectId, int> objCache;
		MemoryCache<std::string, int> strCache;
		std::pair<int, int> entry = { 3, 5 };
		sensateiot::models::ObjectId id;

		objCache.Add({id, 5}, {});

		cache.Add({ 1,1 }, {});
		cache.Add({ 2,1 }, {});
		cache.Add(std::move(entry), {});
		cache.AddOrUpdate({ 1, 2 }, {});
		cache.Remove(3);
		cache.Cleanup();

		auto result = cache.TryGetValue(2);

		std::cout << "Result: " << result.value() << std::endl;

	} catch(std::exception& ex) {
		std::cout << "Unable to complete memory cache test: " << ex.what() << std::endl;
	}
	
	return -EXIT_SUCCESS;
}*/
