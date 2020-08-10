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
#include <sensateiot/data/messagevalidator.h>

#include <boost/algorithm/string/replace.hpp>

#include <string>

namespace sensateiot::http
{
	MessageHandler::MessageHandler(services::MessageService& service) : m_service(service)
	{
	}

	httpd::HttpResponse MessageHandler::HandleRequest(const httpd::HttpRequest& request)
	{
		if (!(request.GetMethod() == boost::beast::http::verb::post ||
			request.GetMethod() == boost::beast::http::verb::options)) {
			return this->HandleInvalidMethod();
		}

		std::string msg(request.GetBody().begin(), request.GetBody().end());
		boost::replace_all(msg, "\r\n", "\n");

		auto result = this->m_validator(msg);

		if(!result.first) {
			return this->HandleUnprocessable();
		}
		
		this->m_service->AddMessage(std::make_pair(std::move(msg), std::move(result.second)));
		
		httpd::HttpResponse response;
		response.Data().assign(AcceptedMessage);
		response.SetStatus(boost::beast::http::status::accepted);
		response.SetKeepAlive(request.GetKeepAlive());

		return response;
	}
}
