/*
 * HTTP response.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <sensateiot/httpd/httpresponse.h>

namespace sensateiot::httpd
{
	std::string& HttpResponse::Data()
	{
		return this->m_data;
	}

	std::string& HttpResponse::ContentType()
	{
		return this->m_contentType;
	}

	std::string& HttpResponse::Server()
	{
		return this->m_server;
	}

	bool HttpResponse::GetKeepAlive() const
	{
		return this->m_keepAlive;
	}

	void HttpResponse::SetKeepAlive(bool keep)
	{
		this->m_keepAlive = keep;
	}

	boost::beast::http::status HttpResponse::GetStatus() const
	{
		return this->m_status;
	}

	void HttpResponse::SetStatus(boost::beast::http::status status)
	{
		this->m_status = status;
	}
}
