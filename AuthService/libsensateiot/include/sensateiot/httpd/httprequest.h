/*
 * HTTP request.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#if defined(WIN32) && !defined(_WIN32_WINNT)
#define _WIN32_WINNT 0x0601
#endif

#include <boost/beast/http.hpp>
#include <boost/beast/core.hpp>

#include <string>

namespace sensateiot::httpd
{
	class HttpRequest {
	public:
		explicit HttpRequest() = default;

		[[nodiscard]]
		boost::string_view GetTarget() const;
		void SetTarget(boost::string_view target);

		[[nodiscard]]
		boost::string_view GetBody() const;
		void SetBody(boost::string_view body);

		[[nodiscard]]
		boost::beast::http::verb GetMethod() const;
		void SetMethod(boost::beast::http::verb method);

	private:
		boost::string_view m_target;
		boost::string_view m_body;
		boost::beast::http::verb m_method {boost::beast::http::verb::get};
	};
}
