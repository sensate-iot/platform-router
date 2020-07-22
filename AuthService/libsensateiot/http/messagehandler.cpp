/*
 * HTTP message handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/httpd/httpresponse.h>
#include <sensateiot/http/abstracthandler.h>
#include <sensateiot/http/messagehandler.h>
#include <sensateiot/stl/referencewrapper.h>

#include <string>

namespace sensateiot::http
{
	MessageHandler::MessageHandler(services::MessageService& service) : m_service(service)
	{
	}

	httpd::HttpResponse MessageHandler::HandleRequest(const httpd::HttpRequest& request)
	{
		return httpd::HttpResponse();
	}
}
