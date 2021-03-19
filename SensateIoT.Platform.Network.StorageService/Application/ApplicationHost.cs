/*
 * Storage client application host.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SensateIoT.Platform.Network.StorageService.Application
{
	public class ApplicationHost
	{
		private readonly ManualResetEvent _reset;
		private readonly IHost m_host;

		public ApplicationHost(IHost host)
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
			var logger = this.m_host.Services.GetRequiredService<ILogger<ApplicationHost>>();
			logger.LogInformation($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");

			this.m_host.RunAsync();
			this._reset.WaitOne();

			logger.LogInformation($"Stopping {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
		}
	}
}
