/*
 * GZIP compression/decompression.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <string>
#include <sstream>

#include <sensateiot/util/gzip.h>
#include <sensateiot/util/base64.h>

#define BOOST_BIND_NO_PLACEHOLDERS

#include <boost/iostreams/filtering_streambuf.hpp>
#include <boost/iostreams/copy.hpp>
#include <boost/iostreams/filter/gzip.hpp>

namespace sensateiot::util
{
	std::string Compress(const std::vector<char>& data)
	{
		namespace bio = boost::iostreams;
		std::vector<char> result;

		bio::filtering_istreambuf is;
		is.push(bio::gzip_compressor());
		is.push(bio::array_source(data.data(), data.size()));
		bio::copy(is, bio::back_inserter(result));

		return Encode64(result);
	}
}
