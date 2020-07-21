/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/services/messageservice.h>
#include <boost/fiber/future/future.hpp>
#include <boost/fiber/future/packaged_task.hpp>

#include <deque>
#include <vector>

#ifdef WIN32
#undef max
#endif

namespace sensateiot::services
{
	MessageService::MessageService(
			mqtt::IMqttClient &client,
			AbstractUserRepository &users,
			AbstractApiKeyRepository &keys,
			AbstractSensorRepository &sensors,
			const config::Config &conf
	) : m_conf(conf), m_index(0), m_count(0),
	    m_keyRepo(keys), m_userRepo(users), m_sensorRepo(sensors)
	{
		std::unique_lock lock(this->m_lock);
		std::string uri = this->m_conf.GetMqtt().GetPrivateBroker().GetBroker().GetUri();

		for(auto idx = 0; idx < this->m_conf.GetWorkers(); idx++) {
			consumers::MeasurementConsumer measurementHandler(client, this->m_cache, conf);
			consumers::MessageConsumer messageHandler(client, this->m_cache, conf);

			this->m_measurementHandlers.emplace_back(std::move(measurementHandler));
			this->m_messageHandlers.emplace_back(std::move(messageHandler));
		}
	}
	
	std::vector<models::ObjectId> MessageService::Process(bool postProcess)
	{
		auto &log = util::Log::GetLog();
		std::vector<models::ObjectId> ids;

		std::deque<boost::fibers::packaged_task<ProcessingStats()>> queue;
		std::vector<boost::fibers::future<ProcessingStats>> results;

		std::shared_lock lock(this->m_lock);

		for(auto idx = 0U; idx < this->m_measurementHandlers.size(); idx++) {
			auto processor = [
				&measurementHandler = this->m_measurementHandlers[idx],
				&messageHandler = this->m_messageHandlers[idx],
				&l = this->m_lock,
				pp = postProcess
			]() {

				try {

					std::shared_lock lock(l);

					if(pp) {
						auto messageResult = messageHandler.PostProcess();
						return std::make_pair(measurementHandler.PostProcess() + messageResult, std::vector<models::ObjectId>());
					}

					auto messageResult = messageHandler.Process();
					auto measurementResult = measurementHandler.Process();

					measurementResult.first += messageResult.first;
					measurementResult.second.reserve(measurementResult.second.size() + messageResult.second.size());
					std::move(
						std::begin(messageResult.second),
						std::end(messageResult.second),
						std::back_inserter(measurementResult.second)
					);

					return measurementResult;
				} catch(std::exception&) {
					return std::pair(std::size_t{}, std::vector < models::ObjectId>{});
				}
			};
			
			boost::fibers::packaged_task<ProcessingStats()> tsk(std::move(processor));

			results.emplace_back(tsk.get_future());
			queue.emplace_back(std::move(tsk));
		}

		lock.unlock();

		while(!queue.empty()) {
			auto front = std::move(queue.front());
			queue.pop_front();

			std::thread exec(std::move(front));
			exec.detach();
		}

		std::size_t authorized = 0ULL;

		try {
			for(auto&& future : results) {
				if(!future.valid()) {
					continue;
				}

				auto tmp = future.get();

				if(ids.empty()) {
					ids = std::move(tmp.second);
				} else {
					ids.resize(ids.size() + tmp.second.size());
					std::move(std::begin(tmp.second), std::end(tmp.second), std::back_inserter(ids));
				}

				authorized += tmp.first;
			}
		} catch(boost::fibers::future_error& error) {
			log << "Unable to get data from future: " << error.what() << util::Log::NewLine;
		} catch(std::system_error& error) {
			log << "Unable to process messages: " << error.what() << util::Log::NewLine;
		}

		if(authorized != 0ULL) {
			log << "Authorized " << std::to_string(authorized) << " messages." << util::Log::NewLine;
		}

		return ids;
	}

	std::time_t MessageService::Process()
	{
		auto &log = util::Log::GetLog();
		auto count = this->m_count.exchange(0ULL);
	
		if (count <= 0) {
			this->m_cache.CleanupFor(CleanupTimeout);
			return {};
		}

		log << "Processing " << std::to_string(count) << " measurements!" << util::Log::NewLine;
		auto start = boost::chrono::system_clock::now();
		auto ids = this->Process(false);

		if(!ids.empty()) {
			this->Load(ids);
		}

		this->Process(true);
		this->m_cache.CleanupFor(CleanupTimeout);

		auto diff = boost::chrono::system_clock::now() - start;
		using Millis = boost::chrono::milliseconds;
		auto duration = boost::chrono::duration_cast<Millis>(diff);

		log << "Processing took: " << std::to_string(duration.count()) << "ms." << util::Log::NewLine;

		return duration.count();
	}

	void MessageService::AddMeasurement(std::string msg)
	{
		auto measurement = this->m_validator(msg);

		if(!measurement.first) {
			return;
		}

		auto pair = std::make_pair(std::move(msg), std::move(measurement.second));

		std::shared_lock lock(this->m_lock);
		std::size_t current = this->m_index.fetch_add(1);

		current %= this->m_measurementHandlers.size();
		++this->m_count;

		auto &repo = this->m_measurementHandlers[current];
		repo.PushMessage(std::move(pair));
	}

	void MessageService::AddMeasurements(std::vector<std::pair<std::string, models::Measurement>> measurements)
	{
		if(measurements.size() > std::numeric_limits<unsigned int>::max()) {
			return;
		}
		
		std::shared_lock lock(this->m_lock);
		std::size_t current = this->m_index.fetch_add(1);

		current %= this->m_measurementHandlers.size();
		this->m_count += static_cast<unsigned int>(measurements.size());

		auto &repo = this->m_measurementHandlers[current];
		repo.PushMessages(std::move(measurements));
	}

	void MessageService::LoadAll()
	{
		auto sensor_f = std::async(std::launch::async, [this]()
		{
			return this->m_sensorRepo->GetAllSensors(0, 0);
		});

		auto user_f = std::async(std::launch::async, [this]()
		{
			return this->m_userRepo->GetAllUsers();
		});

		auto key_f = std::async(std::launch::async, [this]()
		{
			return this->m_keyRepo->GetAllSensorKeys();
		});

		auto sensors = sensor_f.get();
		auto keys = key_f.get();
		auto users = user_f.get();

		auto append_f = std::async(std::launch::async, [&]()
		{
			this->m_cache.Append(sensors);
			this->m_cache.Append(users);
			this->m_cache.Append(keys);
		});

		std::vector<models::ObjectId> objs;
		objs.reserve(sensors.size());

		for (auto& sensor : sensors) {
			objs.push_back(sensor.GetId());
		}

		this->m_cache.AppendBlackList(objs);
		append_f.wait();
	}

	void MessageService::Load(std::vector<models::ObjectId> &objIds)
	{
		std::sort(objIds.begin(), objIds.end(), [](const auto &a, const auto &b) {
			return a.compare(b) < 0;
		});

		auto iter = std::unique(objIds.begin(), objIds.end());
		objIds.resize(static_cast<unsigned long>(std::distance(objIds.begin(), iter)));

		auto sensors = this->m_sensorRepo->GetRange(objIds, 0, 0);
		boost::unordered_set<boost::uuids::uuid> uuids;

		if(sensors.empty()) {
			return;
		}

		for(auto &sensor : sensors) {
			uuids.insert(sensor.GetOwner());
		}

		auto user_f = std::async(std::launch::async, [this, &uuids]()
		{
			return this->m_userRepo->GetRange(uuids);
		});

		auto key_f = std::async(std::launch::async, [this, &sensors]()
		{
			return this->m_keyRepo->GetKeysFor(sensors);
		});

		try {
			auto users = user_f.get();
			auto keys = key_f.get();

			this->m_cache.Append(sensors);
			this->m_cache.Append(users);
			this->m_cache.Append(keys);
			this->m_cache.AppendBlackList(objIds);
		} catch (std::exception& ex) {
			auto& log = util::Log::GetLog();
			log << "PostgreSQL error while fetching users/keys: " << ex.what() << util::Log::NewLine;
		}
	}
}
