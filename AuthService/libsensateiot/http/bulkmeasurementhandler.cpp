/*
 * HTTP measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/httpd/httpresponse.h>
#include <sensateiot/services/messageservice.h>
#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/http/bulkmeasurementhandler.h>

#include <boost/format.hpp>

#include <boost/beast/http.hpp>
#include <string>

namespace sensateiot::http
{
	BulkMeasurementHandler::BulkMeasurementHandler(services::MessageService& service) : m_service(service)
	{
	}

	httpd::HttpResponse BulkMeasurementHandler::HandleRequest(const httpd::HttpRequest& request)
	{
		if (!(request.GetMethod() == boost::beast::http::verb::post ||
			request.GetMethod() == boost::beast::http::verb::options)) {
			return this->HandleInvalidMethod();
		}

		auto result = this->m_validator({ request.GetBody().begin(), request.GetBody().end() });

		if(!result.has_value()) {
			return this->HandleUnprocessable();
		}

		auto reply = boost::format(AcceptedMessage.data()) % result->size();
		this->m_service->AddMeasurements(std::move(*result));

		httpd::HttpResponse response;
		response.Data().assign(reply.str());
		response.SetStatus(boost::beast::http::status::accepted);
		response.SetKeepAlive(request.GetKeepAlive());

		return response;
	}

	httpd::HttpResponse BulkMeasurementHandler::HandleInvalidMethod()
	{
		httpd::HttpResponse response;

		response.Data().assign(R"({"message": "No route has been defined."})");
		response.SetStatus(boost::beast::http::status::method_not_allowed);
		response.SetKeepAlive(false);

		return response;
	}

	httpd::HttpResponse BulkMeasurementHandler::HandleUnprocessable()
	{
		httpd::HttpResponse response;

		response.Data().assign(R"({"message": "Unable to process request."})");
		response.SetStatus(boost::beast::http::status::unprocessable_entity);
		response.SetKeepAlive(false);

		return response;
	}
}
