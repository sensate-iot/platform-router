/*
 * HTTP status handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/http/statushandler.h>

namespace sensateiot::http
{
	StatusHandler::StatusHandler()
	{
		this->m_response.Data().assign(StatusText);
		this->m_response.SetStatus(boost::beast::http::status::ok);
		this->m_response.Server().assign("SensateIoT/AuthService");
		this->m_response.ContentType().assign(ContentType);
	}

	httpd::HttpResponse StatusHandler::HandleRequest(const httpd::HttpRequest& request)
	{
		return this->m_response;
	}
}
