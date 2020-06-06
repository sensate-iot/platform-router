/*
 * Logging stream.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <iostream>

#include <sensateiot.h>
#include <sensateiot/util/log.h>

#include <boost/log/core.hpp>
#include <boost/log/trivial.hpp>
#include <boost/log/sources/severity_logger.hpp>

#include <boost/log/expressions.hpp>
#include <boost/log/sinks/text_file_backend.hpp>

#include <boost/log/utility/setup/file.hpp>
#include <boost/log/utility/setup/console.hpp>
#include <boost/log/utility/setup/common_attributes.hpp>

static constexpr const char *format = "[%TimeStamp%]: %Message%";
static constexpr uint32_t rotation_size = 1024*1024*10;

using namespace boost::log::trivial;
using namespace boost::log;

static sensateiot::config::Logging logging_config;

namespace sensateiot::util
{
	Log::Log()
	{
		auto core = boost::log::core::get();

		boost::log::add_file_log(
			keywords::file_name = logging_config.GetPath(),
			keywords::rotation_size = rotation_size,
			keywords::time_based_rotation = sinks::file::rotation_at_time_point(0, 0, 0),
			keywords::format = format,
			keywords::auto_flush = true
		);

		auto console = boost::log::add_console_log(
				std::cout,
				keywords::format = format,
				keywords::auto_flush = true
		);

		core->set_filter(severity >= info);
		boost::log::add_common_attributes();
	}

	Log& Log::operator<<(const std::string &input)
	{
		std::scoped_lock l(this->m_lock);

		this->m_buffer += input;
		return *this;
	}

	Log& Log::operator<<(NewLineType nl)
	{
		UNUSED(nl);
		this->Flush();
		return *this;
	}

	void Log::Flush()
	{
		std::scoped_lock l(this->m_lock);

		boost::log::sources::severity_logger<boost::log::trivial::severity_level> lg;
		BOOST_LOG_SEV(lg, info) << this->m_buffer;
		this->m_buffer.clear();
	}

	void Log::StartLogging(const config::Logging& logging)
	{
		logging_config = logging;
	}

	Log& Log::GetLog()
	{
		static Log log;
		return log;
	}
}
