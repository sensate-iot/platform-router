/*
 * HTTP bulk message handler.
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
#include <sensateiot/data/bulkmessagevalidator.h>

#include <string>

namespace sensateiot::http
{
	class BulkMessageHandler : public AbstractHandler {
	public:
		explicit BulkMessageHandler() = default;
		explicit BulkMessageHandler(services::MessageService& service);

		httpd::HttpResponse HandleRequest(const httpd::HttpRequest& request) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_service;
		data::BulkMessageValidator m_validator;

		static constexpr auto AcceptedMessage = std::string_view(R"({"message":"Messages queued.","count":%llu})");
	};
}
