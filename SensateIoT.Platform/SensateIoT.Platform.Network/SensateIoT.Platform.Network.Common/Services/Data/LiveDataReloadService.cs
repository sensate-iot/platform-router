/*
 * Live data refresh service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Services.Data
{
	public class LiveDataReloadService : TimedBackgroundService
	{
		private readonly ILogger<LiveDataReloadService> m_logger;

		public LiveDataReloadService(IDataCache cache,
		                             IOptions<DataReloadSettings> settings,
		                             ILogger<LiveDataReloadService> logger) : base(
			settings.Value.StartDelay, settings.Value.LiveDataReloadInterval)
		{
			this.m_logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			throw new System.NotImplementedException();
		}
	}
}
