/*
 * HTTP measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/http/measurementhandler.h>
#include <sensateiot/data/measurementvalidator.h>

#include <string>

namespace sensateiot::http
{
	MeasurementHandler::MeasurementHandler(services::MessageService& service)
	{
	}

	httpd::HttpResponse MeasurementHandler::HandleRequest(const httpd::HttpRequest& request)
	{
		return httpd::HttpResponse();
	}
}
