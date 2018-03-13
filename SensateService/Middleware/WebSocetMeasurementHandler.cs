/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Middleware
{
	public class WebSocketMeasurementHandler : WebSocketHandler
	{
		private readonly IMeasurementRepository _measurements;
		private readonly ISensorRepository _sensors;

		public WebSocketMeasurementHandler(IWebSocketRepository sockets,
			IMeasurementRepository measurements, ISensorRepository sensors) : base(sockets)
		{
			this._measurements = measurements;
			this._sensors = sensors;
		}

		public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg, id;
			dynamic jobj;

			msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

			try {
				jobj = JObject.Parse(msg);
				id = jobj.CreatedById;
				if(id == null) {
					await this.SendMessage(socket, new {status = 404}.ToString());
					return;
				}

				var sensor = await this._sensors.GetAsync(id);
				await this._measurements.ReceiveMeasurement(sensor, msg);
				await this.SendMessage(socket, new {status = 200}.ToString());
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				await this.SendMessage(socket, new {status = 400}.ToString());
			}
		}
	}
}
