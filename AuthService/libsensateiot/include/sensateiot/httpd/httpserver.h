/*
 * HTTP webserver header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef WIN32
#define _WIN32_WINNT 0x0601
#endif

#include <sensateiot/httpd/httpsession.h>
#include <sensateiot/httpd/httprequest.h>
#include <sensateiot/httpd/httplistener.h>

#include <config/config.h>
#include <atomic>

namespace sensateiot::httpd
{
	class HttpServer {
	public:
		typedef HttpListener::HttpRequestHandler HttpRequestHandler;
		typedef HttpListener::HandlerMap HandlerMap;

		HttpServer() = default;
		explicit HttpServer(const config::Config& config);

		void Run();
		void Stop();
		bool Running() const;

		void AddHandler(const std::string& route, const HttpRequestHandler& handler);
		
		template <typename Func>
		void AddHandler(const std::string& route, Func&& handler)
		{
			this->m_handlers.Emplace(route, std::forward<Func>(handler));
		}

	private:
		config::Config m_config{};
		std::atomic_bool m_running;
		HandlerMap m_handlers;
	};
}
