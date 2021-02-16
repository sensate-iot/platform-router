/*
 * A generic service run in the background.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace SensateIoT.Platform.Network.Common.Services.Background
{
	public abstract class BackgroundService : IHostedService, IDisposable
	{
		private Task m_executor;
		protected readonly CancellationTokenSource m_stoppingCts;

		protected BackgroundService()
		{
			this.m_executor = Task.CompletedTask;
			this.m_stoppingCts = new CancellationTokenSource();
		}

		public abstract Task ExecuteAsync(CancellationToken token);

		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			this.m_executor = this.ExecuteAsync(this.m_stoppingCts.Token);

			return this.m_executor.IsCompleted ? this.m_executor : Task.CompletedTask;
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			if(this.m_executor == null) {
				return;
			}

			try {
				this.m_stoppingCts.Cancel();
			} finally {
				await Task.WhenAny(this.m_executor, Task.Delay(Timeout.Infinite, cancellationToken));
			}
		}

		public void Dispose()
		{
			this.m_stoppingCts.Cancel();
		}
	}
}
