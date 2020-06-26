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
	std::string Compress(const std::string& data)
	{
		namespace bio = boost::iostreams;

		std::stringstream compressed;
		std::stringstream origin(data);

		bio::filtering_streambuf<bio::input> out;
		out.push(bio::gzip_compressor(bio::gzip_params(bio::gzip::best_compression)));
		out.push(origin);
		bio::copy(out, compressed);

		return "";
		//return Encode64(compressed.str());
	}
}
