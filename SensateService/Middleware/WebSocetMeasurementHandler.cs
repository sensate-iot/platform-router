/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Infrastructure.Events;
using SensateService.Models.Json.In;

namespace SensateService.Middleware
{
	public class WebSocketMeasurementHandler : WebSocketHandler
	{
		private readonly IMeasurementRepository _measurements;
		private readonly ISensorRepository _sensors;
		private readonly ISensorStatisticsRepository _stats;
		private readonly IServiceProvider _provider;

		public WebSocketMeasurementHandler(IWebSocketRepository sockets,
										   IMeasurementRepository measurements,
										   ISensorRepository sensors,
										   ISensorStatisticsRepository stats, IServiceProvider provider) : base(sockets)
		{
			this._sensors = sensors;
			this._stats = stats;
			this._measurements = measurements;
			this._provider = provider;
		}

		public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg, id;
			RawMeasurement raw;
			AuditLog log;

			msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(msg);
				id = raw.CreatedById;

				if(id == null) {
					await this.SendMessage(socket, new {status = 404}.ToString());
					return;
				}

				using(var scope = this._provider.CreateScope()) {
					var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
					var auditlogs = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
                    var sensor = await this._sensors.GetAsync(id);
                    await this._measurements.ReceiveMeasurement(sensor, raw);

					var user = users.Get(sensor.Owner);
                    log = new AuditLog {
                        Address = IPAddress.Any,
                        Method = RequestMethod.WebSocket,
                        Route = "/measurement",
                        Timestamp = DateTime.Now,
                        Author = user
                    };

                    await auditlogs.CreateAsync(log);
                    await this._stats.IncrementAsync(sensor);
				}

				dynamic jobj = new JObject();
				jobj.status = 200;

				await this.SendMessage(socket, jobj.ToString());
			} catch(InvalidRequestException ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = ex.ErrorCode;
				await this.SendMessage(socket, jobj.ToString());
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = 500;
				await this.SendMessage(socket, jobj.ToString());
			}
		}
	}
}
