/*
 * HTTP request.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/httpd/httprequest.h>
#include <boost/beast/http.hpp>

namespace sensateiot::httpd
{
	boost::beast::string_view HttpRequest::GetTarget() const
	{
		return this->m_target;
	}

	void HttpRequest::SetTarget(boost::beast::string_view target)
	{
		this->m_target = std::move(target);
	}

	boost::string_view HttpRequest::GetBody() const
	{
		return this->m_body;
	}

	void HttpRequest::SetBody(boost::string_view body)
	{
		this->m_body = std::move(body);
	}

	boost::beast::http::verb HttpRequest::GetMethod() const
	{
		return this->m_method;
	}

	void HttpRequest::SetMethod(boost::beast::http::verb method)
	{
		this->m_method = method;
	}
}
