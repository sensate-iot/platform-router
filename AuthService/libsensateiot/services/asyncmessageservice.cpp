/*
 * Asynchronous message service header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <thread>
#include <stdexcept>

#include <sensateiot/services/asyncmessageservice.h>

namespace sensateiot::services
{
	AsyncMessageService::AsyncMessageService(const config::Config& conf, data::DataCache& cache) :
		m_executors(std::thread::hardware_concurrency()), m_config(conf),
		m_handlers(std::thread::hardware_concurrency()), m_cache(cache)
	{
	}
}
