/*
 * HTTP status handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/http/abstracthandler.h>
#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/httpd/httpresponse.h>

#include <string_view>

namespace sensateiot::http
{
	class StatusHandler : public AbstractHandler {
	public:
		explicit StatusHandler();
		httpd::HttpResponse HandleRequest(const httpd::HttpRequest& request) override;

	private:
		httpd::HttpResponse m_response;
		static constexpr std::string_view StatusText = R"({"status":"OK"})";
		static constexpr std::string_view ContentType = R"(application/json)";
	};
}
