/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SensateService.Common.Data.Dto.Generic;
using SensateService.Exceptions;
using SensateService.Infrastructure.Authorization;

namespace SensateService.Ingress.WebSocketHandler.Handlers
{
	public class WebSocketMessageHandler : Middleware.WebSocketHandler
	{
		private readonly ILogger<WebSocketMessageHandler> m_logger;
		private readonly IMessageAuthorizationProxyCache m_proxy;

		public WebSocketMessageHandler(ILogger<WebSocketMessageHandler> logger, IMessageAuthorizationProxyCache auth)
		{
			this.m_logger = logger;
			this.m_proxy = auth;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			try {
				var raw_msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
				this.m_proxy.AddMessage(raw_msg);
			} catch(InvalidRequestException ex) {
				this.m_logger.LogInformation($"Unable to store message: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = ex.ErrorCode;
				await this.SendMessage(socket.Raw, jobj.ToString());
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to store message: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = 500;
				await this.SendMessage(socket.Raw, jobj.ToString());
			}
		}
	}
}
