/*
 * HTTP listener header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef WIN32
#define _WIN32_WINNT 0x0601
#endif

#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/asio.hpp>

#include <sensateiot/httpd/httpsession.h>

#include <unordered_map>
#include <vector>
#include <utility>
#include <memory>
#include <shared_mutex>

namespace sensateiot::httpd
{
	class HttpListener : public std::enable_shared_from_this<HttpListener> {
		using tcp = boost::asio::ip::tcp;

	public:
		typedef HttpSession::HttpRequestHandler HttpRequestHandler;
		typedef HttpSession::HandlerMap HandlerMap;

		explicit HttpListener(HandlerMap& map, boost::asio::io_context& ctx, tcp::endpoint ep);
		void Run();

	private:
		boost::asio::io_context& m_ctx;
		tcp::acceptor m_acceptor;
		HandlerMap& m_handlers;

		void DoAccept();
		void OnAccept(boost::beast::error_code ec, tcp::socket socket);
	};
}
