/*
 * MqttHandler application runner.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace SensateService.Processing.StorageClient.Application
{
	public class Application
	{
		private readonly ManualResetEvent _reset;
		private readonly IHost m_host;

		public Application(IHost host)
		{
			this.m_host = host;
			this._reset = new ManualResetEvent(false);
			Console.CancelKeyPress += this.CancelEvent_Handler;
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this._reset.Set();
			e.Cancel = true;
		}

		public void Run()
		{
			this.m_host.RunAsync();
			this._reset.WaitOne();
			Console.WriteLine("Stopping MqttHandler!");
		}
	}
}
