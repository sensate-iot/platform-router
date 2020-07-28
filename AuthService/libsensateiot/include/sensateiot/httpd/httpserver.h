/*
 * HTTP webserver header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#if defined(WIN32) && !defined(_WIN32_WINNT)
#define _WIN32_WINNT 0x0601
#endif

#include <sensateiot/httpd/httpsession.h>
#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/httpd/httplistener.h>
#include <sensateiot/http/abstracthandler.h>

#include <config/config.h>

#include <limits>
#include <atomic>

namespace sensateiot::httpd
{
	class HttpServer {
	public:
		typedef HttpListener::HttpRequestHandler HttpRequestHandler;
		typedef HttpListener::HandlerMap HandlerMap;

		explicit HttpServer() = default;
		explicit HttpServer(const config::Config& config);

		void Run();
		void Stop();
		bool Running() const;

		void AddHandler(const std::string& route, const HttpRequestHandler& handler);
		void AddHandler(const std::string& route, http::AbstractHandler& handler);
		
	private:
		config::Config m_config{};
		std::atomic_bool m_running;
		HandlerMap m_handlers;

		static constexpr auto HandlerTimeout = std::numeric_limits<long>::max();
	};
}
