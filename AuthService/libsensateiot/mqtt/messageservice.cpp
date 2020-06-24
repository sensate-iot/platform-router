/*
 * MQTT message service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/mqtt/messageservice.h>

#include <future>
#include <thread>
#include <deque>
#include <vector>

namespace sensateiot::mqtt
{
	MessageService::MessageService(
			IMqttClient &client,
			services::AbstractUserRepository &users,
			services::AbstractApiKeyRepository &keys,
			services::AbstractSensorRepository &sensors,
			const config::Config &conf
	) : m_conf(conf), m_index(0), m_cache(), m_count(0),
	    m_keyRepo(keys), m_userRepo(users), m_sensorRepo(sensors)
	{
		std::unique_lock lock(this->m_lock);
		std::string uri = this->m_conf.GetMqtt().GetPrivateBroker().GetBroker().GetUri();

		//for(int idx = 0; idx < conf.GetWorkers(); idx++) {
		for(int idx = 0; idx < 1; idx++) {
			MeasurementHandler handler(client, this->m_cache);
			this->m_handlers.push_back(std::move(handler));
		}
	}

	std::time_t MessageService::Process()
	{
		auto &log = util::Log::GetLog();
		unsigned int count = this->m_count.exchange(0);

		auto start = boost::chrono::system_clock::now();
		log << "Processing " << std::to_string(count) << " measurements!" << util::Log::NewLine;
		std::deque<std::packaged_task<std::vector<models::ObjectId>()>> queue;
		std::shared_lock lock(this->m_lock);

		for(auto &handler : this->m_handlers) {
			std::packaged_task<std::vector<models::ObjectId>()> tsk([&] {
				return handler.Process();
			});

			queue.emplace_back(std::move(tsk));
		}

		std::vector<std::future<std::vector<models::ObjectId>>> results(queue.size());

		for(auto &entry : queue) {
			results.push_back(entry.get_future());
		}

		while(!queue.empty()) {
			auto front = std::move(queue.front());
			queue.pop_front();

			std::thread exec(std::move(front));
			exec.detach();
		}

		std::vector<models::ObjectId> ids;

		try {
			for(auto &future : results) {
				if(!future.valid()) {
					continue;
				}

				future.wait();

				if(ids.empty()) {
					ids = future.get();
				} else {
					auto tmp = future.get();
					ids.resize(ids.size() + tmp.size());
					std::move(std::begin(tmp), std::end(tmp), std::back_inserter(ids));
				}
			}
		} catch(std::future_error& error) {
			auto& log = util::Log::GetLog();
			log << "Unable to get data from future: " << error.what() << util::Log::NewLine;
		}

		if(!ids.empty()) {
			this->Load(ids);
		}

		auto diff = boost::chrono::system_clock::now() - start;
		using Millis = boost::chrono::milliseconds;
		auto duration = boost::chrono::duration_cast<Millis>(diff);

		log << "Processing took: " << std::to_string(duration.count())
		    << "ms." << sensateiot::util::Log::NewLine;

		return duration.count();
	}

	void MessageService::RawProcess()
	{
		std::vector<models::ObjectId> objIds;
		std::unique_lock lock(this->m_lock);

		for(auto &handler : this->m_handlers) {
			auto result = handler.Process();
			std::move(std::begin(result), std::end(result), std::back_inserter(objIds));
		}

		this->Load(objIds);
	}

	void MessageService::AddMeasurement(std::string msg)
	{
		auto measurement = this->m_validator(msg);

		if(!measurement.first) {
			return;
		}

		auto pair = std::make_pair(std::move(msg), std::move(measurement.second));

		auto current = this->m_index.fetch_add(1);
		auto id = current % this->m_handlers.size();
		auto newValue = current % this->m_handlers.size();

		while(!this->m_index.compare_exchange_weak(current, newValue, std::memory_order_relaxed)) {
			newValue = current % this->m_handlers.size();
		}

		this->m_count++;

		std::shared_lock lock(this->m_lock);
		auto &repo = this->m_handlers[id];
		repo.PushMeasurement(std::move(pair));
	}

	void MessageService::Load(std::vector<models::ObjectId> &objIds)
	{
		std::sort(objIds.begin(), objIds.end(), [](const models::ObjectId &a, const models::ObjectId &b) {
			return a.compare(b) < 0;
		});

		auto iter = std::unique(objIds.begin(), objIds.end());
		objIds.resize(static_cast<unsigned long>(std::distance(objIds.begin(), iter)));

		auto sensors = this->m_sensorRepo->GetRange(objIds, 0, 0);
		boost::unordered_set<boost::uuids::uuid> uuids;

		for(auto &sensor : sensors) {
			uuids.insert(sensor.GetOwner());
		}

		auto users = this->m_userRepo->GetRange(uuids);
		auto keys = this->m_keyRepo->GetKeysByOwners(uuids);

		this->m_cache.Append(sensors);
		this->m_cache.Append(users);
		this->m_cache.Append(keys);
	}
}
