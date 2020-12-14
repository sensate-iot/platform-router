/*
 * TriggerService application runner.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;

using Microsoft.Extensions.Hosting;

namespace SensateIoT.Platform.Network.TriggerService.Application
{
	public class AppHost
	{
		private readonly ManualResetEvent _reset;
		private readonly IHost m_host;

		public AppHost(IHost host)
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
