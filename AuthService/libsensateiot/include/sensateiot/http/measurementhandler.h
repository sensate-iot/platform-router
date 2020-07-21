/*
 * HTTP measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/httpd/httpresponse.h>
#include <sensateiot/http/abstracthandler.h>
#include <sensateiot/services/messageservice.h>
#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/data/measurementvalidator.h>

#include <string>

namespace sensateiot::http
{
	class MeasurementHandler : public AbstractHandler {
	public:
		explicit MeasurementHandler() = default;
		explicit MeasurementHandler(services::MessageService& service);
		~MeasurementHandler() override = default;

		httpd::HttpResponse HandleRequest(const httpd::HttpRequest& request) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_service;
		data::MeasurementValidator m_validator;

		static constexpr auto AcceptedMessage = std::string_view(R"({"message":"Measurement queued."})");
	};
}
