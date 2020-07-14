/*
 * HTTP abstract handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/httpd/httpresponse.h>
#include <sensateiot/httpd/httprequest.h>

namespace sensateiot::http
{
	class AbstractHandler {
	public:
		virtual ~AbstractHandler() = default;
		virtual httpd::HttpResponse HandleRequest(const httpd::HttpRequest& request);
	};
}
