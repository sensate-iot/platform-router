/*
 * HTTP measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/http/measurementhandler.h>
#include <sensateiot/data/measurementvalidator.h>

#include <boost/algorithm/string/replace.hpp>
#include <string>

namespace sensateiot::http
{
	MeasurementHandler::MeasurementHandler(services::MessageService& service) : m_service(service)
	{
	}

	httpd::HttpResponse MeasurementHandler::HandleRequest(const httpd::HttpRequest& request)
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

		this->m_service->AddMeasurement(std::make_pair(std::move(msg), std::move(result.second)));

		httpd::HttpResponse response;
		response.Data().assign(AcceptedMessage);
		response.SetStatus(boost::beast::http::status::accepted);
		response.SetKeepAlive(request.GetKeepAlive());
		return response;
	}
}
