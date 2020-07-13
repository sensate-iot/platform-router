/*
 * HTTP response.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef WIN32
#define _WIN32_WINNT 0x0601
#endif

#include <boost/beast/http.hpp>
#include <string>

namespace sensateiot::httpd
{
	class HttpResponse {
	public:
		explicit HttpResponse() = default;

		std::string& Data();
		std::string& ContentType();
		std::string& Server();

		[[nodiscard]]
		bool GetKeepAlive() const;
		void SetKeepAlive(bool keep);

		[[nodiscard]]
		boost::beast::http::status GetStatus() const;
		void SetStatus(boost::beast::http::status status);

	private:
		std::string m_data;
		std::string m_contentType;
		std::string m_server {"SensateIoT_Auth"};
		bool m_keepAlive {true};
		boost::beast::http::status m_status {boost::beast::http::status::ok};
	};
}
