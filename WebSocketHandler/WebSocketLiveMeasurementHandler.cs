/*
 * Live websocket handlers.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Middleware;
using SensateService.Models;

namespace SensateService.WebSocketHandler
{
	public class WebSocketLiveMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly ConcurrentDictionary<string, SortedSet<AuthenticatedWebSocket>> sockets;
		private readonly ILogger<WebSocketLiveMeasurementHandler> _logger;
		private readonly IServiceProvider _provider;

		public WebSocketLiveMeasurementHandler(IWebSocketRepository sockets,
			IServiceProvider sp,
			ILogger<WebSocketLiveMeasurementHandler> logger) :
			base(sockets)
		{
			this.sockets = new ConcurrentDictionary<string, SortedSet<AuthenticatedWebSocket>>();
			this._logger = logger;
			this._provider = sp;

			MqttInternalMeasurementHandler.MeasurementReceived += MeasurementReceived_Handler;
		}

		public async Task MeasurementReceived_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			Debug.WriteLine("Measurement received!");
			await Task.CompletedTask;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;
			SensorAuthorizationRequest rq;
			Sensor sensor;
			SensateUser user;

			msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
			try {
				rq = JsonConvert.DeserializeObject<SensorAuthorizationRequest>(msg);

				if(rq == null)
					return;
			} catch(JsonSerializationException ex) {
				this._logger.LogWarning($"Unable to deserialize request: ${ex.Message}");
				return;
			}

			using(var scope = this._provider.CreateScope()) {
				var sensors = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
				var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();


				user = await users.GetByClaimsPrincipleAsync(socket.Authentication.Principal);
				sensor = await sensors.GetAsync(rq.SensorID);

				if(sensor.Secret != rq.SensorSecret || user.Id != sensor.Owner) {
					await socket.Raw.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unable to authorize sensor!", CancellationToken.None);
					return;
				}

				this.sockets.AddOrUpdate(
					rq.SensorID,
					new SortedSet<AuthenticatedWebSocket> {socket},
					(key, set) => {
					set.Add(socket);
					return set;
				});
				this._logger.LogDebug("Added socket to the socket set!");
			}

			/*if(this.sockets.TryGetValue(rq.SensorID, out var sensors)) {
				sensors.Add(socket);
			} else {
				var s = new SortedSet<WebSocket> {socket};

				if(!this.sockets.TryAdd(rq.SensorID, s)) {
					this.sockets.get
				}
			}*/
		}
	}
}