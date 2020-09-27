/*
 * HTTP message handler.
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
#include <sensateiot/data/messagevalidator.h>

#include <string>

namespace sensateiot::http
{
	class MessageHandler : public AbstractHandler {
	public:
		explicit MessageHandler() = default;
		explicit MessageHandler(services::MessageService& service);
		~MessageHandler() override = default;

		httpd::HttpResponse HandleRequest(const httpd::HttpRequest& request) override;

	private:
		stl::ReferenceWrapper<services::MessageService> m_service;
		data::MessageValidator m_validator;

		static constexpr auto AcceptedMessage = std::string_view(R"({"message":"Message queued."})");
	};
}
