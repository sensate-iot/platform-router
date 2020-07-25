/*
 * HTTP worker thread.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */


#include <sensateiot/httpd/httpsession.h>
#include <sensateiot/httpd/httplistener.h>
#include <sensateiot/httpd/httpserver.h>
#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/util/log.h>

#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/beast/version.hpp>
#include <boost/asio.hpp>

#include <forward_list>

namespace beast = boost::beast;
namespace net = boost::asio;

namespace sensateiot::httpd
{
	HttpServer::HttpServer() : m_handlers(HandlerTimeout)
	{
	}

	HttpServer::HttpServer(const config::Config& config) : m_config(config), m_handlers(HandlerTimeout)
	{
	}

	void HttpServer::Run()
	{
		const auto addr = net::ip::make_address(this->m_config.GetBindAddress());
		const auto port = this->m_config.GetHttpPort();
		const auto threads = 4;

		net::io_context ctx{threads};
		auto sh = std::make_shared<HttpListener>(this->m_handlers, ctx, boost::asio::ip::tcp::endpoint{ addr, port });
		sh->Run();

		net::signal_set sigs(ctx, SIGINT, SIGTERM);
		sigs.async_wait([&](const beast::error_code&, int) {
			ctx.stop();
		});

		std::vector<std::thread> v;
		v.reserve(threads - 1);

		for(auto idx = 0; idx < threads - 1; idx++) {
			v.emplace_back([&ctx]() {
				ctx.run();
			});
		}

		ctx.run();

		for(auto& t : v) {
			t.join();
		}
	}
	
	void HttpServer::AddHandler(const std::string& route, const HttpRequestHandler& handler)
	{
		this->m_handlers.Insert(route, handler);
	}

	void HttpServer::AddHandler(const std::string& route, http::AbstractHandler& handler)
	{
		this->AddHandler(route, [&h = handler](const httpd::HttpRequest& req) {
			return h.HandleRequest(req);
		});
	}

	void HttpServer::Stop()
	{
	}

	bool HttpServer::Running() const
	{
		return this->m_running.load();
	}
}
