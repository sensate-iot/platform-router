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

#include <mongoc.h>

#include <sensateiot/models/sensor.h>
#include <boost/uuid/random_generator.hpp>

#include <sensateiot/util/log.h>
#include <sensateiot/stl/map.h>

//static constexpr int MapSize     = 15000000;
//static constexpr int LookupSize  = 100000;
//static constexpr int DeleteSize  = 50000;
//static constexpr int DeleteStart = 150000;

static constexpr int MapSize = 100000;
static constexpr int DeleteSize = MapSize;
static constexpr int DeleteStart = 0;
static constexpr int LookupSize = 10000;

static std::vector<boost::uuids::uuid> sensorOwnerIds;
static std::vector<sensateiot::models::ObjectId> sensorIds;
static std::vector<std::string> sensorStringIds;

static void set_test()
{
	using namespace sensateiot::detail;
	using SetType = sensateiot::stl::Set<int>;
	SetType t;
	SetType t2;
	int x = 1;

	t.Insert(x);
	t.Emplace(4);

	namespace ids = boost::uuids;
	ids::random_generator gen;
	sensateiot::stl::Set<ids::uuid> id_set;

	id_set.Emplace(gen());

	assert(t.Has(4));
	assert(!t.Has(5));

	t2 = t;
	t = std::move(t2);
}

static void timeout_test()
{
	using namespace sensateiot::stl;
	using TreeType = Map<std::string, int>;

	TreeType t(500);
	TreeType t2(500);

	t.Emplace(std::to_string(1), 1000);
	t.Emplace(std::to_string(2), 2000);
	t.Emplace(std::to_string(3), 3000);
	t.Emplace(std::to_string(4), 4000);

	auto v2 = t.At(std::to_string(2));
	auto v1 = t.At(std::to_string(1));
	auto v3 = t.Find(std::to_string(3));

	if(v1 != 1000 || v2 != 2000 || *v3 != 3000) {
		throw;
	}

	using namespace std::chrono_literals;
	std::this_thread::sleep_for(2s);
	auto& log = sensateiot::util::Log::GetLog();

	t["4"] = 5000;

	if(t["4"] != 5000) {
		throw;
	}

	try {
		v2 = t.At(std::to_string(2));
		(void)v2;
	} catch(std::out_of_range& ex) {
		(void)ex;
		log << "Correctly timed out!" << sensateiot::util::Log::NewLine;
	}

	t2 = t;
	t = t2;

	std::this_thread::sleep_for(550ms);

	t["10"] = 10;
	t.Cleanup(boost::chrono::milliseconds(50));

	assert(t.Contains("10"));
}

static void concurrent_test()
{
	static constexpr int MapSize = 10000;
	static constexpr int LookupSize = 5000;

	using namespace sensateiot::stl;
	using TreeType = Map<std::string, int>;
	TreeType t;

	auto t1 = std::thread([&t]() {
		static constexpr int StartInsert = 0;

		for(auto idx = StartInsert; idx < MapSize; idx++) {
			t.Insert(std::to_string(idx), idx);
		}

		auto now = boost::chrono::high_resolution_clock::now();
		for(auto idx = StartInsert; idx < LookupSize; idx++) {
			auto value = t.At(std::to_string(idx), now);
			(void)value;
		}

		for(auto idx = StartInsert; idx < LookupSize; idx++) {
			t.Erase(std::to_string(idx));
		}
	});

	auto t2 = std::thread([&t]() {
		static constexpr int StartInsert = MapSize;

		for(auto idx = StartInsert; idx < MapSize*2; idx++) {
			t.Insert(std::to_string(idx), idx);
		}

		auto now = boost::chrono::high_resolution_clock::now();
		for(auto idx = StartInsert; idx < LookupSize+StartInsert; idx++) {
			auto value = t.At(std::to_string(idx), now);
			(void)value;
		}

		for(auto idx = StartInsert; idx < LookupSize+StartInsert; idx++) {
			t.Erase(std::to_string(idx));
		}
	});

	auto t3 = std::thread([&t]() {
		static constexpr int StartInsert = MapSize*2;
		for(auto idx = StartInsert; idx < MapSize*3; idx++) {
			t.Insert(std::to_string(idx), idx);
		}

		for(auto idx = StartInsert; idx < LookupSize+StartInsert; idx++) {
			t.Erase(std::to_string(idx));
		}

		auto now = boost::chrono::high_resolution_clock::now();
		for(auto idx = LookupSize+StartInsert; idx < MapSize*3; idx++) {
			auto value = t.At(std::to_string(idx), now);
			(void)value;
		}
	});

	auto t4 = std::thread([&t]() {
		static constexpr int StartInsert = MapSize*3;
		for(auto idx = StartInsert; idx < MapSize*4; idx++) {
			t.Insert(std::to_string(idx), idx);
		}

		for(auto idx = StartInsert; idx < MapSize*4; idx++) {
			auto iter = t.Find(std::to_string(idx));
			t.Erase(iter);
//			t.Erase(std::to_string(idx));
		}
	});

	t1.join();
	t2.join();
	t3.join();
	t4.join();

	if(!t.Validate()) {
		throw;
	}
}

static void iteration_test()
{
	using namespace sensateiot::stl;
	using TreeType = Map<std::string, int>;
	TreeType t1;
	TreeType t2;

	auto& log = sensateiot::util::Log::GetLog();

	for(auto idx = 0; idx < 25; idx++) {
		auto id = std::to_string(idx);
		auto id_t2 = std::to_string(idx+10);

		t1.Emplace(std::move(id), idx);
		t2.Emplace(std::move(id_t2), idx+10);
	}

	t1.Merge(std::move(t2));

	log << "Tree entries:";
	for(auto iter = t1.Begin(); iter != t1.End(); ++iter) {
		log << " " << std::to_string(*iter);
	}
	log << sensateiot::util::Log::NewLine;

	auto begin = t1.Begin();
	auto end = t1.Begin();
	++end;
	++end;
	++end;
	++end;

	t1.Erase(begin, end);

	log << "Tree entries:";
	for(auto iter = t1.Begin(); iter != t1.End(); ++iter) {
		log << " " << std::to_string(*iter);
	}
	log << sensateiot::util::Log::NewLine;

	assert(t1 == t1);
}

static void test_insert()
{
	using namespace sensateiot::detail;
//	using TreeType = sensateiot::stl::Map<std::string, sensateiot::models::Sensor>;
	using TreeType = sensateiot::stl::Map<sensateiot::models::ObjectId, sensateiot::models::Sensor>;
//	using TreeType = sensateiot::stl::Map<sensateiot::models::ObjectId::ObjectIdType, sensateiot::models::Sensor>;
	TreeType t;

//	std::vector<std::string> keys;

	auto& log = sensateiot::util::Log::GetLog();

	log << "Start map test!" << sensateiot::util::Log::NewLine;
	using ClockType = std::chrono::high_resolution_clock ;

	ClockType::time_point start = ClockType::now();
	using Millis = std::chrono::milliseconds;
	using Nanos = std::chrono::nanoseconds;

	for(auto idx = 0UL; idx < MapSize; idx++) {
		sensateiot::models::Sensor s;
//		auto id = std::to_string(idx);
		auto id = sensorIds[idx];
//		keys.push_back(id);

		s.SetId(sensorIds[idx]);
		s.SetOwner(sensorOwnerIds[idx]);
		s.SetSecret(std::to_string(idx * 10));

		t.Emplace(std::move(id), std::move(s));
	}

	ClockType::time_point end = ClockType::now();

	log << "End of map test!" << sensateiot::util::Log::NewLine;
	auto diff = end - start;
	log << "Insertion took: " << std::to_string(std::chrono::duration_cast<Millis>(diff).count())
		<< "ms." << sensateiot::util::Log::NewLine;
	log << "Insertion took: " << std::to_string(std::chrono::duration_cast<Nanos>(diff).count() / MapSize)
	    << "ns per entry." << sensateiot::util::Log::NewLine;


	t.Validate();

	log << "Start lookup-map test!" << sensateiot::util::Log::NewLine;

	using tp = boost::chrono::high_resolution_clock::time_point ;
	auto l = [&t]()  {
		tp now = boost::chrono::high_resolution_clock::now();

		for(auto idx = 0UL; idx < LookupSize; idx++) {
			sensateiot::models::ObjectId id(sensorStringIds[idx]);
//			auto sensor = t.At(sensorIds[idx], now);
			auto sensor = t.At(id, now);
//			auto sensor = t.Find(keys.at(idx), now);
			(void)sensor.GetId();
		}
	};

	start = ClockType::now();

//	l();
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

	log << "Pre-delete size: " << std::to_string(t.Size()) << sensateiot::util::Log::NewLine;
	log << "Start delete test!" << sensateiot::util::Log::NewLine;

	for(auto idx = DeleteStart; idx < (DeleteStart+DeleteSize); idx++) {
		auto sensor = t.Find(sensorIds[static_cast<unsigned long>(idx)]);

		if(sensor == t.End()) {
			continue;
		}

		t.Erase(sensor);
	}

	log << "Post-delete size: " << std::to_string(t.Size()) << sensateiot::util::Log::NewLine;

	t.Validate();
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
		}

		test_insert();
		iteration_test();
		set_test();
		concurrent_test();
		timeout_test();
	} catch(std::exception&) {
		std::exit(1);
	}

	return -EXIT_SUCCESS;
}
