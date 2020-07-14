/*
 * HTTP server.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <config/config.h>

#include <sensateiot/httpd/httpserver.h>
#include <sensateiot/http/statushandler.h>
#include <sensateiot/util/log.h>

#include <cstdlib>

int main(int argc, char** argv)
{
	using namespace sensateiot;
	config::Config config;
	http::StatusHandler status;

	config.SetBindAddress("127.0.0.1");
	config.SetHttpPort(8080);
	config.GetLogging().SetPath("auth-service_%N.log");

	util::Log::StartLogging(config.GetLogging());
	auto& log = util::Log::GetLog();

	log << "Starting HTTP server..." << util::Log::NewLine;
	httpd::HttpServer server(config);

	server.AddHandler("/v1/status", [&s = status](const auto& request) {
		return s.HandleRequest(request);
	});

	server.AddHandler("/index.json", [](const httpd::HttpRequest& request)
	{
		httpd::HttpResponse response;

		response.Data().assign(R"({"status":"OK"})");
		response.ContentType().assign("application/json");

		return response;
	});
	server.Run();

	return -EXIT_SUCCESS;
}
