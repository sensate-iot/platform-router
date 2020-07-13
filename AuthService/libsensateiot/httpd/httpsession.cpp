/*
 * HTTP worker thread.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#ifdef WIN32
#define _WIN32_WINNT 0x0601
#endif

#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/beast/version.hpp>
#include <boost/asio.hpp>

#include <sensateiot/httpd/httpsession.h>
#include <sensateiot/util/log.h>

namespace beast = boost::beast;
namespace http = beast::http;
using tcp = boost::asio::ip::tcp;

namespace sensateiot::httpd
{
	template< class Body, class Allocator, class Send>
	void handle_request(http::request<Body, http::basic_fields<Allocator>>&& req,
		Send&& send, const HttpSession::HandlerMap& handlers)
	{
		auto const bad_request =
			[&req](beast::string_view why)
		{
			http::response<http::string_body> res{ http::status::bad_request, req.version() };
			res.set(http::field::server, BOOST_BEAST_VERSION_STRING);
			res.set(http::field::content_type, "text/html");
			res.keep_alive(req.keep_alive());
			res.body() = std::string(why);
			res.prepare_payload();
			return res;
		};

		auto const not_found =
			[&req](beast::string_view target)
		{
			http::response<http::string_body> res{ http::status::not_found, req.version() };
			res.set(http::field::server, BOOST_BEAST_VERSION_STRING);
			res.set(http::field::content_type, "text/html");
			res.keep_alive(req.keep_alive());
			res.body() = "The resource '" + std::string(target) + "' was not found.";
			res.prepare_payload();
			return res;
		};

		auto const server_error =
			[&req](beast::string_view what)
		{
			http::response<http::string_body> res{ http::status::internal_server_error, req.version() };
			res.set(http::field::server, BOOST_BEAST_VERSION_STRING);
			res.set(http::field::content_type, "text/html");
			res.keep_alive(req.keep_alive());
			res.body() = "An error occurred: '" + std::string(what) + "'";
			res.prepare_payload();
			return res;
		};

		try {
			std::string target(req.target().begin(), req.target().end());
			HttpRequest request;

			request.SetMethod(req.method());
			request.SetTarget(req.target());
			request.SetBody(req.body());

			handlers.Process(target, [&](const HttpSession::HttpRequestHandler& handler) {
				auto resp = handler(request);

				http::response<http::string_body> res{ resp.GetStatus(), req.version() };

				res.set(http::field::content_type, resp.ContentType());
				res.set(http::field::server, resp.Server());
				res.body().assign(resp.Data());
				res.content_length(resp.Data().length());
				res.keep_alive(req.keep_alive());
				res.prepare_payload();

				send(std::move(res));
			});
		} catch(std::out_of_range&) {
			send(not_found(req.target()));
		}
	}

	bool HttpSession::ResponseQueue::IsFull() const
	{
		return this->m_items.size() >= Limit;
	}

	bool HttpSession::ResponseQueue::OnWrite()
	{
		assert(!this->m_items.empty());

		auto full = this->IsFull();
		this->m_items.erase(this->m_items.begin());

		if(!this->m_items.empty()) {
			(*this->m_items.front())();
		}

		return full;
	}

	HttpSession::HttpSession(tcp::socket&& socket, HandlerMap& handlers) :
		stream_(std::move(socket)), queue_(*this), m_handlers(handlers)
	{
	}

	void HttpSession::Run()
	{
		dispatch(
			stream_.get_executor(),
			beast::bind_front_handler(
				&HttpSession::DoRead,
				this->shared_from_this()
			)
		);
	}

	void HttpSession::OnWrite(bool close, beast::error_code ec, std::size_t bytes_transferred)
	{
		boost::ignore_unused(bytes_transferred);
		
		if(ec) {
			return;
		}

		if(close) {
			return this->DoClose();
		}

		if(this->queue_.OnWrite()) {
			this->DoRead();
		}
	}

	void HttpSession::OnRead(beast::error_code ec, std::size_t count)
	{
		boost::ignore_unused(count);

		if(ec == http::error::end_of_stream) {
			return this->DoClose();
		}

		if(ec) {
			return;
		}

		handle_request(this->parser_->release(), this->queue_, this->m_handlers);

		if(!this->queue_.IsFull()) {
			this->DoRead();
		}
	}

	void HttpSession::DoRead()
	{
		this->parser_.emplace();
		this->parser_->body_limit((std::numeric_limits<std::uint64_t>::max)());
		this->stream_.expires_after(Timout);

		http::async_read(
			this->stream_,
			this->buffer_,
			*this->parser_,
			beast::bind_front_handler(
				&HttpSession::OnRead,
				this->shared_from_this()
			)
		);
	}

	void HttpSession::DoClose()
	{
		beast::error_code ec;
		this->stream_.socket().shutdown(tcp::socket::shutdown_send, ec);
	}
}
