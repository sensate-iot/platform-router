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
#include <sensateiot/http/measurementhandler.h>

#include <boost/beast/http.hpp>
#include <string>

namespace sensateiot::http
{
	MeasurementHandler::MeasurementHandler(services::MessageService& service) : m_service(service)
	{
	}

	httpd::HttpResponse MeasurementHandler::HandleRequest(const httpd::HttpRequest& request)
	{
		httpd::HttpResponse response;

		response.Data().clear();
		response.Server().assign("Sensate IoT/AuthService");
		response.ContentType().assign("application/json");
		response.SetStatus(boost::beast::http::status::accepted);

		return response;
	}
}
