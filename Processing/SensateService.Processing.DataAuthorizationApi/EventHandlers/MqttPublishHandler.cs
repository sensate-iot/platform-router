/*
 * MQTT publish handler for bulk storage.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using SensateService.Common.Config.Settings;
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization;
using SensateService.Infrastructure.Events;
using SensateService.Services.Processing;

namespace SensateService.Processing.DataAuthorizationApi.EventHandlers
{
	public class MqttPublishHandler : IHostedService
	{
		private readonly IServiceProvider m_provider;
		private readonly DataAuthorizationSettings m_settings;

		public MqttPublishHandler(IServiceProvider provider, IOptions<DataAuthorizationSettings> settings)
		{
			this.m_provider = provider;
			this.m_settings = settings.Value;
		}

		private async Task MessagesAuthorized_Handler(object sender, DataAuthorizedEventArgs e)
		{
			using var scope = this.m_provider.CreateScope();
			var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();
			var result = GetPublishableData(e);

			await client.PublishOnAsync(this.m_settings.MessageTopic, result, false);
		}

		private async Task MeasurementsAuthorized_Handler(object sender, DataAuthorizedEventArgs e)
		{
			using var scope = this.m_provider.CreateScope();
			var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();
			var result = GetPublishableData(e);

			await client.PublishOnAsync(this.m_settings.MeasurementTopic, result, false);
		}

		private static string GetPublishableData(DataAuthorizedEventArgs e)
		{
			using var stream = new MemoryStream();
			e.Data.WriteTo(stream);

			return stream.ToArray().Compress();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			AuthorizationCache.MeasurementDataAuthorized += this.MeasurementsAuthorized_Handler;
			AuthorizationCache.MessageDataAuthorized += this.MessagesAuthorized_Handler;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			AuthorizationCache.MeasurementDataAuthorized -= this.MeasurementsAuthorized_Handler;
			AuthorizationCache.MessageDataAuthorized -= this.MessagesAuthorized_Handler;
			return Task.CompletedTask;
		}
	}
}
