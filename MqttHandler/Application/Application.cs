/*
 * MqttHandler application runner.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading;

using Microsoft.Extensions.Hosting;

namespace SensateService.MqttHandler.Application
{
	public class Application
	{
		private readonly ManualResetEvent _reset;
		private readonly IHost m_host;
		private byte m_wait;

		public Application(IHost host)
		{
			this.m_wait = 0;
			this.m_host = host;
			this._reset = new ManualResetEvent(false);
			Console.CancelKeyPress += this.CancelEvent_Handler;
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this.m_wait += 1;

			if(this.m_wait == 2) {
				this._reset.Set();
				e.Cancel = true;
			} else {
				Console.WriteLine("Press CTRL-C again to quit.");
			}
		}

		public void Run()
		{
			this.m_host.RunAsync();
			Console.WriteLine("Stopping MqttHandler!");
			this._reset.WaitOne();
		}
	}
}
