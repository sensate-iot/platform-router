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
using System.Linq;
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
using SensateService.WebSocketHandler.Data;

namespace SensateService.WebSocketHandler.Handlers
{
	public class WebSocketLiveMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly ILogger<WebSocketLiveMeasurementHandler> _logger;
		private readonly IServiceProvider _provider;
		private readonly WebSocketSensorMap _map;

		public WebSocketLiveMeasurementHandler(IServiceProvider sp, ILogger<WebSocketLiveMeasurementHandler> logger)
		{
			this._logger = logger;
			this._provider = sp;
			this._map = new WebSocketSensorMap();

			MqttInternalMeasurementHandler.MeasurementReceived += MeasurementReceived_Handler;
		}

		public async Task MeasurementReceived_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			string id;
			string json;
			ArraySegment<byte> buffer;
			IEnumerable<AuthenticatedWebSocket> sockets;
			int idx;
			Task[] workers;

			id = e.Measurement.CreatedBy.ToString();
			sockets = this._map.Get(id);
			var authenticatedWebSockets = sockets.ToList();

			workers = new Task[authenticatedWebSockets.Count()];
			idx = 0;

			json = e.Measurement.ToJson();
			var data = Encoding.UTF8.GetBytes(json);
			buffer = new ArraySegment<byte>(data);

			foreach(var socket in authenticatedWebSockets) {
				if(socket.Raw.State != WebSocketState.Open)
					break;

				workers[idx] = socket.Raw.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
				idx++;
			}

			await Task.WhenAll(workers);
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;
			SensorAuthorizationRequest rq;
			Sensor sensor;
			SensateUser user;

			if(socket.Authentication == null || !socket.IsAuthenticated()) {
				await socket.Raw.CloseAsync(
					WebSocketCloseStatus.PolicyViolation,
					"Unable to authenticate client!",
					CancellationToken.None
				);
				return;
			}

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
					await socket.Raw.CloseAsync(WebSocketCloseStatus.PolicyViolation,
						"Unable to authenticate!", CancellationToken.None);
					return;
				}

				this._map.Add(sensor, socket);
				this._logger.LogDebug("Added socket to the socket set!");
			}
		}

		public override void OnConnected(WebSocket socket)
		{
			this._logger.LogDebug("Live websocket connected..");
		}

		public override async void OnDisconnected(AuthenticatedWebSocket socket)
		{
			this._logger.LogDebug("Live websocket disconnected!");
			this._map.Remove(socket);
			await socket.Raw.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service side close", CancellationToken.None);
		}

		public override async void OnForceClose(AuthenticatedWebSocket socket)
		{
			this._logger.LogDebug("Live websocket force closed!");
			this._map.Remove(socket);
			await socket.Raw.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service side close", CancellationToken.None);
		}
	}
}