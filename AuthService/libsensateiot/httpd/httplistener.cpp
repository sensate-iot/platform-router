/*
 * HTTP listener implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#ifdef WIN32
#define _WIN32_WINNT 0x0601
#endif

#include <sensateiot/httpd/httplistener.h>
#include <sensateiot/httpd/httpsession.h>
#include <sensateiot/util/log.h>

#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/asio.hpp>

namespace beast = boost::beast;
namespace net = boost::asio;
using tcp = boost::asio::ip::tcp;

namespace sensateiot::httpd
{
	HttpListener::HttpListener(HandlerMap& handlers, boost::asio::io_context& ctx, tcp::endpoint ep) :
		m_ctx(ctx), m_acceptor(make_strand(ctx)), m_handlers(handlers)
	{
		beast::error_code ec;
		auto& log = util::Log::GetLog();

		m_acceptor.open(ep.protocol(), ec);
		
		if(ec) {
			log << "Unable to open end point!" << util::Log::NewLine;
			return;
		}

		m_acceptor.set_option(net::socket_base::reuse_address(true), ec);

		if(ec) {
			log << "Unable to set socket options!" << util::Log::NewLine;
			return;
		}

		m_acceptor.bind(ep, ec);

		if(ec) {
			log << "Unable to bind end point!" << util::Log::NewLine;
			return;
		}

		m_acceptor.listen(net::socket_base::max_listen_connections, ec);

		if(ec) {
			log << "Unable to listen on end point!" << util::Log::NewLine;
			return;
		}
	}

	void HttpListener::Run()
	{
		net::dispatch(
			this->m_acceptor.get_executor(),
			beast::bind_front_handler(
				&HttpListener::DoAccept,
				this->shared_from_this()
			)
		);
	}

	void HttpListener::DoAccept()
	{
		this->m_acceptor.async_accept(
			make_strand(this->m_ctx),
			beast::bind_front_handler(
				&HttpListener::OnAccept,
				this->shared_from_this()
			)
		);
	}

	void HttpListener::OnAccept(beast::error_code ec, tcp::socket socket)
	{
		if(ec) {
			auto& log = util::Log::GetLog();
			log << "Unable to accept socket!" << util::Log::NewLine;
		} else {
			std::make_shared<HttpSession>(std::move(socket), this->m_handlers)->Run();
		}

		this->DoAccept();
	}
}
