/*
 * A websocket which has been authenticated successfully.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Authentication;

namespace SensateService.Middleware
{
	public class AuthenticatedWebSocket
	{
		public WebSocket Raw { get; set; }
		public AuthenticateResult Authentication { get; set; }
		public string Id { get; }

		public bool IsAuthenticated() => this.Authentication.Succeeded && !this.Authentication.None;

		public AuthenticatedWebSocket()
		{
			this.Id = Guid.NewGuid().ToString();
		}
	}
}