/*
 * Measurement handler, which serves the internal measurement topic.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Infrastructure.Events;
using SensateService.Middleware;
using SensateService.Models;

namespace SensateService.WebSocketHandler.Handlers
{
	public class MqttInternalMeasurementHandler : MqttHandler
	{
		public static event OnMeasurementReceived MeasurementReceived;

		public override void OnMessage(string topic, string msg)
		{
			this.OnMessageAsync(topic, msg).RunSynchronously();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			Measurement m;
			MeasurementReceivedEventArgs args;

			m = Measurement.FromJson(message);
			args = new MeasurementReceivedEventArgs {
				Measurement = m,
				CancellationToken = CancellationToken.None
			};

			await InvokeReceiveMeasurement(null, args);
		}

		private static async Task InvokeReceiveMeasurement(Sensor sensor, MeasurementReceivedEventArgs eventargs)
		{
			Delegate[] delegates;

			if(MeasurementReceived == null)
				return;

			delegates = MeasurementReceived.GetInvocationList();

			if(delegates.Length <= 0)
				return;

			await MeasurementReceived.Invoke(sensor, eventargs);
		}
	}
}