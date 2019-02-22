/*
 * MqttHandler application runner.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SensateService.Helpers;

namespace SensateService.MqttHandler.Application
{
	public class Application
	{
		private readonly ManualResetEvent _reset;
		private IServiceProvider sp;

		public Application(IServiceProvider sp)
		{
			this._reset = new ManualResetEvent(false);
			this.sp = sp;
			Console.CancelKeyPress += this.CancelEvent_Handler;
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this._reset.Set();
			e.Cancel = true;
		}

		public void Run()
		{
			var services = this.sp.GetServices<IHostedService>().ToList();

			services.ForEach(x => x.StartAsync(CancellationToken.None));
			Console.WriteLine("MQTT client started");
			this._reset.WaitOne();

			services = this.sp.GetServices<IHostedService>().ToList();
			Task.Run(async () => {
				foreach(var hosted in services) {
					await hosted.StopAsync(CancellationToken.None).AwaitSafely();
				}
			}).Wait();
		}
	}
}
