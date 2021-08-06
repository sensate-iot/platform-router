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
using Microsoft.Extensions.Logging;

namespace SensateIoT.Platform.Router.Common.Services.Background
{
	public abstract class BackgroundService : IHostedService, IDisposable
	{
		private Task m_executor;
		private readonly ILogger m_logger;

		protected readonly CancellationTokenSource m_stoppingCts;

		protected BackgroundService(ILogger logger)
		{
			this.m_executor = Task.CompletedTask;
			this.m_stoppingCts = new CancellationTokenSource();
			this.m_logger = logger;
		}

		protected abstract Task ExecuteAsync(CancellationToken token);

		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			var merged = CancellationTokenSource.CreateLinkedTokenSource(this.m_stoppingCts.Token, cancellationToken);
			this.m_executor = this.InternalExecuteAsync(merged.Token);

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

		private async Task InternalExecuteAsync(CancellationToken ct)
		{
			try {
				await this.ExecuteAsync(ct).ConfigureAwait(false);
			} catch(Exception ex) {
				this.m_logger.LogCritical(ex, "Unable to complete background service execution!");
				Environment.Exit(1);
			}
		}

		public void Dispose()
		{
			this.m_stoppingCts.Cancel();
		}
	}
}
