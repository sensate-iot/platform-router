/*
 * HTTP worker thread header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef WIN32
#define _WIN32_WINNT 0x0601
#endif

#include <boost/asio.hpp>

#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>

#include <sensateiot/stl/map.h>
#include <sensateiot/httpd/httpresponse.h>
#include <sensateiot/httpd/httprequest.h>

#include <chrono>
#include <string>
#include <memory>
#include <string_view>

namespace sensateiot::httpd
{
	struct Work;
	class HttpSession : public std::enable_shared_from_this<HttpSession> {
		using tcp = boost::asio::ip::tcp;
		static constexpr auto Timout = std::chrono::seconds(30);

		class ResponseQueue {
			using tcp = boost::asio::ip::tcp;
			static constexpr std::size_t Limit = 8;

			struct Work {
				/* Work item after type erasion */
				virtual ~Work() = default;
				virtual void operator()() = 0;
			};

		public:
			explicit ResponseQueue(HttpSession& session) : m_session(session), m_items()
			{
			}

			[[nodiscard]]
			bool IsFull() const;
			bool OnWrite();

			template <bool Req, typename B, typename F>
			void operator()(boost::beast::http::message<Req, B, F>&& msg)
			{
				struct WorkImpl : Work {
					HttpSession& m_session;
					boost::beast::http::message<Req, B, F> m_msg;

					WorkImpl(HttpSession& self, boost::beast::http::message<Req, B, F>&& msg)
						: m_session(self) , m_msg(std::move(msg))
					{
					}

					void operator()() override
					{
						boost::beast::http::async_write(
							this->m_session.stream_,
							this->m_msg,
							boost::beast::bind_front_handler(
								&HttpSession::OnWrite,
								this->m_session.shared_from_this(),
								this->m_msg.need_eof()
							)
						);
					}
				};

				this->m_items.push_back(boost::make_unique<WorkImpl>(this->m_session, std::move(msg)));

				if(this->m_items.size() == 1) {
					(*this->m_items.front())();
				}
			}

		private:
			HttpSession& m_session;
			std::vector<std::unique_ptr<Work>> m_items;
		};


	public:
		typedef std::function<HttpResponse(const HttpRequest&)> HttpRequestHandler;
		typedef stl::Map<std::string, HttpRequestHandler> HandlerMap;
		
		explicit HttpSession(tcp::socket&& socket, HandlerMap& handlers);
		void Run();

	private:
		void OnWrite(bool close, boost::beast::error_code ec, std::size_t bytes_transferred);
		void OnRead(boost::beast::error_code ec, std::size_t count);
		void DoRead();
		void DoClose();

		friend class ResponseQueue;

		boost::beast::tcp_stream stream_;
		boost::beast::flat_buffer buffer_;
		ResponseQueue queue_;
		boost::optional<boost::beast::http::request_parser<boost::beast::http::string_body>> parser_;
		stl::Map<std::string, HttpRequestHandler>& m_handlers;
	};
}
