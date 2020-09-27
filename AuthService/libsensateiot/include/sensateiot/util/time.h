/*
 * Date/time utility library.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <boost/date_time/posix_time/posix_time.hpp>
#include <string>

namespace sensateiot::util
{
	inline std::string GetIsoTimestamp()
	{
		auto time = boost::posix_time::microsec_clock::universal_time();
		return boost::posix_time::to_iso_extended_string(time) + "Z";
	}
}
