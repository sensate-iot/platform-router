/*
 * Asynchronous message service header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/mqtt/measurementhandler.h>
#include <sensateiot/data/datacache.h>
#include <sensateiot/data/measurementvalidator.h>

#include <config/config.h>

#include <vector>
#include <shared_mutex>


namespace sensateiot::services
{
	class AsyncMessageService {
	public:
		explicit AsyncMessageService(const config::Config& conf, data::DataCache& cache);
		
	private:
		std::vector<std::thread> m_executors;
		std::shared_mutex m_lock;
		config::Config m_config;
		
		std::vector<mqtt::MeasurementHandler> m_handlers;

		stl::ReferenceWrapper<data::DataCache> m_cache;
		data::MeasurementValidator m_validator;
	};
}
